using BepInEx;
using R2API.Utils;
using RoR2;
using BepInEx.Configuration;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.ColorUtility;
using static LobbyAppearanceImprovements.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using R2API;
using UnityEngine.Networking;
using System.Reflection;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;
using System.Linq;
using System.Collections.Generic;
using EntityStates;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.Projectile;
using static UnityEngine.Animator;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace LobbyAppearanceImprovements
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "LobbyAppearanceImprovements";
        public const string ModGuid = "com.DestroyedClone.LobbyAppearanceImprovements";
        public static ConfigEntry<string> DefaultColor { get; set; }
        public static ConfigEntry<bool> StopWave { get; set; }
        public static ConfigEntry<bool> PostProcessing { get; set; }
        public static ConfigEntry<bool> HideFade { get; set; }
        public static ConfigEntry<bool> MeshProps { get; set; }
        public static ConfigEntry<bool> SurvivorsInLobby { get; set; }

        //scaling
        public static ConfigEntry<float> CharacterPadScale { get; set; }

        public void Awake()
        {
            //default new Color32((byte)0.981, (byte)0.356, (byte)0.356, (byte)1.000)
            //250.155, 90.78, 90.78
            DefaultColor = Config.Bind("Lights", "Hex Color", "#fa5a5a", "Change the default color of the light");
            StopWave = Config.Bind("Lights", "Disable FlickerLight", true, "Makes the light not flicker anymore.");
            PostProcessing = Config.Bind("Overlay", "Disable Post Processing", true, "Disables the blurry post processing.");
            HideFade = Config.Bind("Overlay", "Hide Fade", true, "There's a dark fade on the top and bottom, this disables it.");
            MeshProps = Config.Bind("Background", "Hide MeshProps", false, "Hides all the meshprops, giving a unique look.");
            SurvivorsInLobby = Config.Bind("Background", "Survivors In Lobby", true, "Shows survivors in the lobby");

            CharacterPadScale = Config.Bind("Background", "Character Display Scale", 0.5f, "Resizes character displays. "); //def 1f
            CommandHelper.AddToConsoleWhenReady();

            if (StopWave.Value || MeshProps.Value || PostProcessing.Value)
            {
                On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;

                On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += CharacterSelectController_OnNetworkUserLoadoutChanged;
            }

        }

        private void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            if (TryParseHtmlString(DefaultColor.Value, out Color color))
                Helpers.ChangeLobbyLightColor(color);
            if (StopWave.Value)
            {
                GameObject.Find("Directional Light").gameObject.GetComponent<FlickerLight>().enabled = false;
            }
            if (MeshProps.Value)
            {
                GameObject.Find("HANDTeaser").SetActive(false);
                GameObject.Find("MeshProps").SetActive(false);
                GameObject.Find("HumanCrate1Mesh").SetActive(false);
                GameObject.Find("HumanCrate2Mesh").SetActive(false);
                GameObject.Find("HumanCanister1Mesh").SetActive(false);
            } else
            {
                if (SurvivorsInLobby.Value)
                {
                    var component = self.gameObject.AddComponent<BackgroundCharacterDisplayToggler>();
                    var characterHolder = new GameObject("HOLDER: Characters");


                    CreateDisplay("Commando", new Vector3(2.65f, 0.01f, 6.00f), new Quaternion(0, 0.9063078f, 0, 0), characterHolder.transform);
                    CreateDisplay("Huntress", new Vector3(4.8f, 1.43f, 15.36f), new Quaternion(0, 0.8788171f, 0, 0), characterHolder.transform);
                    CreateDisplay("Toolbot", new Vector3(-0.21f, 0.15f, 20.84f), new Quaternion(0, 0.999947f, 0, 0), characterHolder.transform);
                    CreateDisplay("Engi", new Vector3(-2.58f, -0.01f, 19f), new Quaternion(0, 0.9925462f, 0, 0), characterHolder.transform);
                    CreateDisplay("Mage", new Vector3(3.35f, 0.21f, 14.73f), new Quaternion(0, 0.9990482f, 0, 0), characterHolder.transform);
                    CreateDisplay("Merc", new Vector3(-1.32f, 3.65f, 22.28f), new Quaternion(0, 0.9993908f, 0, 0), characterHolder.transform);
                    CreateDisplay("Treebot", new Vector3(-6.51f, -0.11f, 22.93f), new Quaternion(0, 0.9816272f, 0, 0), characterHolder.transform);
                    CreateDisplay("Loader", new Vector3(5.04f, 0, 14.26f), new Quaternion(0, 212f/360f, 0, 0), characterHolder.transform);
                    CreateDisplay("Croco", new Vector3(4.83f, 3.59f, 23.58f), new Quaternion(0, 0.9571933f, 0, 0), characterHolder.transform);
                    CreateDisplay("Captain", new Vector3(2.21f, 0.01f, 19.40f), new Quaternion(0, 0.9743701f, 0, 0), characterHolder.transform);
                    //modded
                    CreateDisplay("SniperClassic", new Vector3(-5f, 0f, 22f), new Quaternion(0, -0.9911869f, 0, 0), characterHolder.transform);
                    CreateDisplay("Enforcer", new Vector3(3.2f, 0f, 18.74f), new Quaternion(0, 0.9974989f, 0, 0), characterHolder.transform);
                    CreateDisplay("NemesisEnforcer", new Vector3(4.1f, 2.28f, 20.71f), new Quaternion(0, 0.9774526f, 0, 0), characterHolder.transform);
                    CreateDisplay("Bandit", new Vector3(-3.17f, -0.06f, 5.85f), new Quaternion(0, 0.8698728f, 0, 0), characterHolder.transform); //todo
                    CreateDisplay("Miner", new Vector3(-2.82f, 0.04f, 6.69f), new Quaternion(0, 0.8698728f, 0, 0), characterHolder.transform);
                    CreateDisplay("RobPaladin", new Vector3(-4f, 0.01f, 22f), new Quaternion(0, 0.05399338f, 0, 0), characterHolder.transform);
                    CreateDisplay("CHEF", new Vector3(1.63f, 3.74f, 23.2f), new Quaternion(0, 0.867048f, 0, 0), characterHolder.transform);
                    CreateDisplay("Henry", new Vector3(-3.55f, 1.35f, 7.39f), new Quaternion(0, -0.9444403f, 0, 0), characterHolder.transform);
                }
            }
            if (PostProcessing.Value)
            {
                GameObject.Find("PP").SetActive(false);
            }
            if (HideFade.Value)
            {
                var ui = GameObject.Find("CharacterSelectUI").transform;
                ui.Find("BottomSideFade").gameObject.SetActive(false);
                ui.Find("TopSideFade").gameObject.SetActive(false);
            }
            if (CharacterPadScale.Value != 1f)
            {
                GameObject.Find("CharacterPadAlignments").transform.localScale *= CharacterPadScale.Value;
            }
            if (true)
            {
                var ui = GameObject.Find("CharacterSelectUI").transform.Find("SafeArea").transform;
                ui.Find("LeftHandPanel (Layer: Main)").transform.localScale *= 0.5f;
                var rtSide = ui.Find("RightHandPanel");
                rtSide.localScale *= 0.5f;
                //rtSide.position = new Vector3(80, 30, 90);
            }
        }

        private void CharacterSelectController_OnNetworkUserLoadoutChanged(On.RoR2.UI.CharacterSelectController.orig_OnNetworkUserLoadoutChanged orig, RoR2.UI.CharacterSelectController self, NetworkUser networkUser)
        {
            orig(self, networkUser);
            UpdateBackground(self);
        }

        private void UpdateBackground(RoR2.UI.CharacterSelectController self)
        {
            if (self && self.gameObject.GetComponent<BackgroundCharacterDisplayToggler>())
            {
                var selectedCharacters = new List<SurvivorIndex>();
                var component = self.gameObject.GetComponent<BackgroundCharacterDisplayToggler>();
                foreach (var currentDisplays in self.characterDisplayPads)
                {
                    selectedCharacters.Add(currentDisplays.displaySurvivorIndex);
                }
                foreach (var display in component.backgroundCharacters)
                {
                    // iterate here through the cached list to update whats hidden and whats not with the section below
                }
                component.HANDTeaser.SetActive(showTeaser);
            }
        }

        public class BackgroundCharacterDisplayToggler: MonoBehaviour
        {
            public List<GameObject> backgroundCharacters = new List<GameObject>(0);
        }
    }

    public static class Commands
    {

        [ConCommand(commandName = "changelight", flags = ConVarFlags.ExecuteOnServer, helpText = "changelight {r} {g} {b} {a} | only works in the lobby")]
        public static void ChangeLight(ConCommandArgs args)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
                Helpers.ChangeLobbyLightColor(new Color32((byte)args.GetArgInt(0), (byte)args.GetArgInt(1), (byte)args.GetArgInt(2), (byte)args.GetArgInt(3)));
        }
    }

    public static class Helpers
    {
        public static void ChangeLobbyLightColor(Color32 color)
        {
            GameObject.Find("Directional Light").gameObject.GetComponent<Light>().color = color;
        }
        public static GameObject CreateDisplay(string bodyPrefabName, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyPrefabName+"Body");
            if (bodyPrefab)
            {
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                GameObject displayPrefab = survivorDef.displayPrefab;
                var gameObject = UnityEngine.Object.Instantiate<GameObject>(displayPrefab, position, rotation, parent);

                switch (bodyPrefabName)
                {
                    case "Croco":
                        gameObject.transform.Find("mdlCroco").transform.Find("Spawn").transform.Find("FloorMesh").gameObject.SetActive(false);
                        break;

                }
                return gameObject;
            }
            else Debug.Log("Could not find body "+ bodyPrefabName + "Body");
            return null;
        }
    }
}
