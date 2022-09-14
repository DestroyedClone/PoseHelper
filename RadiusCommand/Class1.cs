using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.RoR2Content;
using System;
using System.Collections.Generic;
using System.Linq;
using RoR2.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Security;
using System.Security.Permissions;
using R2API;
using R2API.Networking;
using EntityStates.AI;
using R2API.Networking.Interfaces;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace RadiusCommand
{
    [BepInPlugin("com.DestroyedClone.RadiusCommand", "Radius Command", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(NetworkingAPI))]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> ModifierKey { get; set; }
        public static ConfigEntry<float> Radius { get; set; }
        public static ConfigEntry<bool> AllowScroll { get; set; }

        public static float finalRadius = 0;
        public static GameObject Indicator;

        public void Awake()
        {
            NetworkingAPI.RegisterMessageType<Networking.RadiusCommandToServer>();


            ModifierKey = Config.Bind("", "Modifier Key", KeyCode.LeftShift, "Hold this button to affect all command essence in your desired radius.");
            Radius = Config.Bind("", "Radius", 5f, "Desired starting radius.");
            AllowScroll = Config.Bind("", "Allow Scrollwheel", true, "If true, then you can modify the radius of the region with the scrollwheel.");

            //On.RoR2.PickupPickerController.SubmitChoice += PickupPickerController_SubmitChoice;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.PickupPickerController.SubmitChoice += PickupPickerController_SubmitChoice1;
            On.RoR2.PickupPickerController.FixedUpdate += PickupPickerController_FixedUpdate;
        }

        private void PickupPickerController_FixedUpdate(On.RoR2.PickupPickerController.orig_FixedUpdate orig, PickupPickerController self)
        {
            return;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
            {
                var comp = obj.gameObject.AddComponent<RadiusCommandComponent>();
                comp.characterBody = obj;
            }
        }

        private void PickupPickerController_SubmitChoice1(On.RoR2.PickupPickerController.orig_SubmitChoice orig, PickupPickerController self, int choiceIndex)
        {
            var CommandCubes = InstanceTracker.GetInstancesList<PickupPickerController>();
            CommandCubes.Remove(self);
            var displayToken = self.GetComponent<GenericDisplayNameProvider>().displayToken;
            var isModifying = Input.GetKey(ModifierKey.Value);


            if (isModifying && self.networkUIPromptController.currentParticipantMaster) //2nd check to prevent recursion from the forloop
            {
                string choices = "";

                // Collect Cubes
                for (int i = 0; i < CommandCubes.Count; i++)
                {
                    var cube = CommandCubes[i];
                    if (Vector3.Distance(self.gameObject.transform.position, cube.gameObject.transform.position) <= finalRadius)
                    {
                        if (cube.GetComponent<GenericDisplayNameProvider>()?.displayToken == displayToken)
                        {
                            var network = cube.networkUIPromptController;
                            if (!network.currentParticipantMaster) //prevents recursion and theft
                            {
                                choices += $"{i},";
                            }
                        }
                    }
                }

                //Send Request
                if (!choices.IsNullOrWhiteSpace())
                {
                    new Networking.RadiusCommandToServer(choices, choiceIndex).Send(NetworkDestination.Server);
                }

            }
            orig(self, choiceIndex);
        }

        private void PickupPickerController_SubmitChoice(On.RoR2.PickupPickerController.orig_SubmitChoice orig, PickupPickerController self, int choiceIndex)
        {
            var CommandCubes = InstanceTracker.GetInstancesList<PickupPickerController>();
            CommandCubes.Remove(self);
            var displayToken = self.GetComponent<GenericDisplayNameProvider>().displayToken;
            var isModifying = Input.GetKey(ModifierKey.Value);

            if (isModifying)
            {
                Debug.Log("we are modifying");
                if (!NetworkServer.active)
                {
                    Debug.Log("We are a client!");
                    var i = 0;
                    foreach (var cube in CommandCubes.ToList())
                    {
                        Debug.Log("Entered foreach loop with cube # "+i);
                        if (Vector3.Distance(self.gameObject.transform.position, cube.gameObject.transform.position) <= Radius.Value)
                        {
                            Debug.Log("Cube is in distance");
                            if (cube.GetComponent<GenericDisplayNameProvider>()?.displayToken == displayToken)
                            {
                                Debug.Log("The display token is valid, Writing the network options.");
                                NetworkWriter networkWriter = cube.networkUIPromptController.BeginMessageToServer();
                                networkWriter.Write(0);
                                networkWriter.Write(choiceIndex);
                                cube.networkUIPromptController.FinishMessageToServer(networkWriter);
                            }
                        } //get rid of the return or it wont work in multiplayer
                        i++;
                    }
                }
                else
                    foreach (var cube in CommandCubes.ToList())
                    {
                        if (Vector3.Distance(self.gameObject.transform.position, cube.gameObject.transform.position) <= Radius.Value)
                        {
                            if (cube.GetComponent<GenericDisplayNameProvider>()?.displayToken == displayToken)
                            {
                                cube.HandlePickupSelected(choiceIndex);
                            }
                        }
                    }
            }
            Debug.Log("We've exited.");
            orig(self, choiceIndex);
        }

        public class RadiusCommandComponent : MonoBehaviour
        {
            public GameObject areaIndicatorInstance;
            public CharacterBody characterBody;

            public void Start()
            {
                finalRadius = Radius.Value;
            }

            public void OnDestroy()
            {
                DestroyIndicator();
            }

            public void Update()
            {
                if (Input.GetKeyDown(ModifierKey.Value))
                {
                    if (!areaIndicatorInstance)
                    {
                        areaIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                        areaIndicatorInstance.transform.position = characterBody.corePosition;
                    }
                }
                if (Input.GetKeyUp(ModifierKey.Value))
                {
                    DestroyIndicator();
                }
                if (Input.mouseScrollDelta.y != 0)
                {
                    if (Input.mouseScrollDelta.y > 0)
                    {
                        finalRadius += 1f;
                    } else
                    {
                        finalRadius = Mathf.Max(0, finalRadius - 1f);
                    }
                }
            }

            public void DestroyIndicator()
            {
                if (areaIndicatorInstance)
                    Destroy(areaIndicatorInstance);
            }

            public void FixedUpdate()
            {
                if (areaIndicatorInstance && Input.GetKey(ModifierKey.Value))
                {
                    areaIndicatorInstance.transform.position = characterBody.corePosition;
                    areaIndicatorInstance.transform.localScale = Vector3.one * finalRadius;
                }
            }
        }
    }
}