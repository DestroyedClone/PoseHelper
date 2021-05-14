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

using EntityStates.AI;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace RadiusCommand
{
    [BepInPlugin("com.DestroyedClone.RadiusCommand", "Radar Command", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> ModifierKey { get; set; }
        public static ConfigEntry<float> Radius { get; set; }

        public void Awake()
        {
            ModifierKey = Config.Bind("", "Modifier Key", KeyCode.LeftShift, "Hold this button to affect all command essence in your desired radius.");
            Radius = Config.Bind("", "Radius", 5f, "Desired radius.");

            //On.RoR2.PickupPickerController.SubmitChoice += PickupPickerController_SubmitChoice;
            On.RoR2.PickupPickerController.SubmitChoice += PickupPickerController_SubmitChoice1;

            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
        }

        private void PickupPickerController_SubmitChoice1(On.RoR2.PickupPickerController.orig_SubmitChoice orig, PickupPickerController self, int choiceIndex)
        {
            var CommandCubes = InstanceTracker.GetInstancesList<PickupPickerController>();
            CommandCubes.Remove(self);
            var displayToken = self.GetComponent<GenericDisplayNameProvider>().displayToken;
            var isModifying = Input.GetKey(ModifierKey.Value);

            
            if (isModifying && self.networkUIPromptController.currentParticipantMaster) //2nd check to prevent recursion from the forloop
            {
                foreach (var cube in CommandCubes.ToList())
                {
                    if (Vector3.Distance(self.gameObject.transform.position, cube.gameObject.transform.position) <= Radius.Value)
                    {
                        if (cube.GetComponent<GenericDisplayNameProvider>()?.displayToken == displayToken)
                        {
                            var network = cube.networkUIPromptController;
                            if (!network.currentParticipantMaster) //prevents recursion and theft
                            {
                                network.currentParticipantMaster = self.networkUIPromptController.currentParticipantMaster;
                                cube.SubmitChoice(choiceIndex);
                            }
                        }
                    }
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
    }
}
