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
        public static ConfigEntry<float> UIScale { get; set; }
        public static ConfigEntry<int> SelectViewMode { get; set; }

        public Dictionary<string, float[]> textCameraSettings = new Dictionary<string, float[]>
        {
            {"Commando", new float[]{ 20, 2, 24 } },
            {"Huntress", new float[]{ 9, -3, 18 } },
            {"Toolbot", new float[]{ 9, 0, 0 } },
            {"Engi", new float[]{ 6, 1, -7.5f } },
            {"Mage", new float[]{ 8, -1, 13 } },
            {"Merc", new float[]{ 5, -8.5f, -3 } },
            {"Treebot", new float[]{ 6, 0.7f, 15.5f } },
            {"Loader", new float[]{ 11, 0, 20 } },
            {"Croco", new float[]{ 8, -8.5f, 13 } },
            {"Captain", new float[]{ 8, 0, 7 } },
            {"SniperClassic", new float[]{ 6, 0.5f, 12.5f } },
            {"Enforcer", new float[]{ 11, -1, 10 } },
            {"NemesisEnforcer", new float[]{ 10, -7.5f, 8 } },
            {"BanditReloaded", new float[]{ 20, 1, -30 } },
            {"HANDOverclocked", new float[]{ 0, 0, 0 } }, //todo
            {"Miner", new float[]{ 17, 1, -26 } },
            {"RobPaladin", new float[]{ 9, -1, -10 } },
            {"CHEF", new float[]{ 5, -8.5f, 3 } },
            {"RobHenry", new float[]{ 12, -7, -27 } },
            {"Wyatt", new float[]{ 0, 0, 0 } }, //todo
            {"Executioner", new float[]{ 0, 0, 0 } }, //todo
        };

        public Dictionary<SurvivorIndex, float[]> characterCameraSettings = new Dictionary<SurvivorIndex, float[]>();

        bool hasSetupCameraValues = false;

        public void Awake()
        {
            //default new Color32((byte)0.981, (byte)0.356, (byte)0.356, (byte)1.000)
            //250.155, 90.78, 90.78
            // Lights
            DefaultColor = Config.Bind("Lights", "Hex Color", "#fa5a5a", "Change the default color of the light");
            StopWave = Config.Bind("Lights", "Disable FlickerLight", true, "Makes the light not flicker anymore.");

            //UI
            PostProcessing = Config.Bind("UI", "Disable Post Processing", true, "Disables the blurry post processing.");
            HideFade = Config.Bind("UI", "Hide Fade", true, "There's a dark fade on the top and bottom, this disables it.");
            UIScale = Config.Bind("UI", "UI Scale", 1f, "Resizes the UIs on the left and right."); //def 1f

            //BG
            MeshProps = Config.Bind("Background", "Hide MeshProps", false, "Hides all the meshprops, giving a unique look.");
            SurvivorsInLobby = Config.Bind("Background", "Survivors In Lobby", true, "Shows survivors in the lobby");
            CharacterPadScale = Config.Bind("Background", "Character Display Scale", 1f, "Resizes character displays. "); //def 1f

            //other
            SelectViewMode = Config.Bind("Other", "Select View Mode (Requires SurvivorsInLobby)", 0, "0 = None" +
                "\n1 = Disappear on selection" +
                "\n2 = Zoom on selection"); //def 1f

            CommandHelper.AddToConsoleWhenReady();

            if (StopWave.Value || MeshProps.Value || PostProcessing.Value)
            {
                On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;
            }

            switch (SelectViewMode.Value)
            {
                case 0: //no effect
                    break;
                case 1: //disappear
                    On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += HideOnSelected;
                    break;
                default: //zoom
                    On.RoR2.UI.CharacterSelectController.SelectSurvivor += ZoomOnSelected;
                    break;
            }

            /*foreach (var entry in textCameraSettings) //move to survivor catalog setup?
            {
                var bodyPrefab = GetBodyPrefab(entry.Key);
                if (bodyPrefab)
                {
                    SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                    SurvivorIndex survivorIndex = survivorDef.survivorIndex;
                    textCameraSettings.TryGetValue(entry.Key, out float[] cameraSetting);
                    characterCameraSettings.Add(survivorIndex, cameraSetting);
                } else
                {
                }
            }
            foreach (var entry in characterCameraSettings)
            {
                Debug.Log(entry.Key + " : " + entry.Value);
            }
            Debug.Log(characterCameraSettings);*/
        }

        private void ZoomOnSelected(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, RoR2.UI.CharacterSelectController self, SurvivorIndex survivor)
        {
            orig(self, survivor);
            var cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();
            if (characterCameraSettings.TryGetValue(survivor, out float[] cameraParams))
            {
                SetCamera(cameraRig, cameraParams[0], cameraParams[1], cameraParams[2]);
            } else
            {
                SetCamera(cameraRig);
            }
        }

        private void SetCamera(CameraRigController cameraRig, float fov = 60f, float pitch = 0f, float yaw = 0f)
        {
            cameraRig.baseFov = fov;
            cameraRig.currentFov += 30f;
            cameraRig.pitch = pitch;
            cameraRig.yaw = yaw;
        }

        private void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            var dirtycomp = self.gameObject.AddComponent<DirtyCam>();
            dirtycomp.cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();

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
                    var dict = component.survivorDisplays;

                    CreateDisplayMaster("Commando", new Vector3(2.65f, 0.01f, 6.00f), new Vector3(0f, 240f,0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Huntress", new Vector3(4.8f, 1.43f, 15.36f), new Vector3(0f, 200f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Toolbot", new Vector3(-0.21f, 0.15f, 20.84f), new Vector3(0f, 170f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Engi", new Vector3(-2.58f, -0.01f, 19f), new Vector3(0f, 150f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Mage", new Vector3(3.35f, 0.21f, 14.73f), new Vector3(0f, 220f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Merc", new Vector3(-1.32f, 3.65f, 22.28f), new Vector3(0f, 180f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Treebot", new Vector3(-6.51f, -0.11f, 22.93f), new Vector3(0f, 140f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Loader", new Vector3(5.04f, 0, 14.26f), new Vector3(0f, 220f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Croco", new Vector3(5f, 3.59f, 22f), new Vector3(0f, 210f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("Captain", new Vector3(2.21f, 0.01f, 19.40f), new Vector3(0f, 190f, 0f), characterHolder.transform, dict);
                    //modded
                    CreateDisplayMaster("SniperClassic", new Vector3(-5f, 0f, 22f), new Vector3(0f, 180f, 0f), characterHolder.transform, dict);
                    //enforcer
                    CreateDisplayMaster("Enforcer", new Vector3(3.2f, 0f, 18.74f), new Vector3(0f, 220f, 0f), characterHolder.transform, dict);
                    CreateDisplayMaster("NemesisEnforcer", new Vector3(3f, 2.28f, 21f), new Vector3(0f, 200f, 0f), characterHolder.transform, dict);
                    //banditreloaded
                    CreateDisplayMaster("BanditReloaded", new Vector3(-3.5f, -0.06f, 5.85f), new Vector3(0f, 154f, 0f), characterHolder.transform, dict);
                    //HAND
                    CreateDisplayMaster("HANDOverclocked", new Vector3(-1.57f, -0.038f, 20.48f), new Vector3(0f, 154f, 0f), characterHolder.transform, dict);
                    //miner
                    CreateDisplayMaster("Miner", new Vector3(-3.3f, 0.04f, 6.69f), new Vector3(0f, 140f, 0f), characterHolder.transform, dict);
                    //Paladin
                    CreateDisplayMaster("RobPaladin", new Vector3(-4f, 0.01f, 22f), new Vector3(0f, 160f, 0f), characterHolder.transform, dict);
                    //CHEF
                    CreateDisplayMaster("CHEF", new Vector3(1.63f, 3.4f, 23.2f), new Vector3(0f, 270f, 0f), characterHolder.transform, dict);
                    //henrymod
                    CreateDisplayMaster("RobHenry", new Vector3(-4.5f, 1.22f, 8.81f), new Vector3(0f, 128f, 0f), characterHolder.transform, dict);
                    //cloudburst
                    CreateDisplayMaster("Wyatt", new Vector3(-3.92f, 0.1f, 9.62f), new Vector3(0f, 138f, 0f), characterHolder.transform, dict);
                    //star storm
                    CreateDisplayMaster("Executioner", new Vector3(1.19f, 0f, 19.74f), new Vector3(0f, 192f, 0f), characterHolder.transform, dict);
                    //chirr here
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
            if (UIScale.Value != 1f)
            {
                var ui = GameObject.Find("CharacterSelectUI").transform.Find("SafeArea").transform;
                ui.Find("LeftHandPanel (Layer: Main)").transform.localScale *= UIScale.Value;
                var rtSide = ui.Find("RightHandPanel");
                rtSide.localScale *= UIScale.Value;
                //rtSide.position = new Vector3(80, 30, 90);
            }
        }

        private void HideOnSelected(On.RoR2.UI.CharacterSelectController.orig_OnNetworkUserLoadoutChanged orig, RoR2.UI.CharacterSelectController self, NetworkUser networkUser)
        {
            orig(self, networkUser);
            UpdateBackground(self);
        }

        private void UpdateBackground(RoR2.UI.CharacterSelectController self)
        {
            if (self && self.gameObject.GetComponent<BackgroundCharacterDisplayToggler>())
            {
                //var selectedCharacters = new List<SurvivorIndex>();
                var component = self.gameObject.GetComponent<BackgroundCharacterDisplayToggler>();

                // Re-enable everything
                foreach (var backgroundCharacters in component.survivorDisplays)
                {
                    backgroundCharacters.Value.SetActive(true);
                }

                foreach (var currentDisplays in self.characterDisplayPads)
                {
                    var index = currentDisplays.displaySurvivorIndex;
                    component.survivorDisplays.TryGetValue(index, out GameObject objectToToggle);
                    objectToToggle.SetActive(false);
                    //selectedCharacters.Add(currentDisplays.displaySurvivorIndex);
                }
                //component.HANDTeaser.SetActive(showTeaser);
            }
        }

        public class DirtyCam : MonoBehaviour
        {
            public CameraRigController cameraRig;
            public float fov = 60f;
            public float pitch = 0f;
            public float yaw = 0f;
            public bool reset = false;

            public void Awake()
            {
                fov = 60f;
                pitch = 0f;
                yaw = 0f;
                reset = false;
                enabled = false;
            }

            public void FixedUpdate()
            {
                if (reset)
                {
                    Awake();
                    return;
                }
                cameraRig.baseFov = fov;
                cameraRig.pitch = pitch;
                cameraRig.yaw = yaw;
            }
        }

        public class BackgroundCharacterDisplayToggler: MonoBehaviour
        {
            //public List<GameObject> backgroundCharacters = new List<GameObject>(0);
            public Dictionary<SurvivorIndex, GameObject> survivorDisplays = new Dictionary<SurvivorIndex, GameObject>();
        }
        public class CameraTweenController : MonoBehaviour
        {
            public CameraRigController cameraRig;
            public float stopwatch = 0f;
            public float incrementValue = 0.05f;
            public float slerpValue = 0f;

            public void Update()
            {

            }
        }
        public void CreateDisplayMaster(string bodyPrefabName, Vector3 position, Vector3 rotation, Transform parent = null, Dictionary<SurvivorIndex, GameObject> keyValuePairs = null)
        {
            var bodyPrefab = GetBodyPrefab(bodyPrefabName);
            if (bodyPrefab)
            {
                var display = CreateDisplay(bodyPrefabName, position, rotation, parent);
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                keyValuePairs.Add(survivorDef.survivorIndex, display);

                if (!hasSetupCameraValues)
                {
                    SurvivorIndex survivorIndex = survivorDef.survivorIndex;
                    textCameraSettings.TryGetValue(bodyPrefabName, out float[] cameraSetting);
                    characterCameraSettings.Add(survivorIndex, cameraSetting);
                }

            }
        }
        public static GameObject GetBodyPrefab(string bodyPrefabName)
        {
            switch (bodyPrefabName)
            {
                case "CHEF":
                    break;
                default:
                    bodyPrefabName += "Body";
                    break;
            }
            var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyPrefabName);
            return bodyPrefab;
        }

        public static GameObject CreateDisplay(string bodyPrefabName, Vector3 position, Vector3 rotation, Transform parent = null)
        {
            var bodyPrefab = GetBodyPrefab(bodyPrefabName);

            SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
            GameObject displayPrefab = survivorDef.displayPrefab;
            var gameObject = UnityEngine.Object.Instantiate<GameObject>(displayPrefab, position, Quaternion.Euler(rotation), parent);
            switch (bodyPrefabName)
            {
                case "Croco":
                    gameObject.transform.Find("mdlCroco")?.transform.Find("Spawn")?.transform.Find("FloorMesh")?.gameObject.SetActive(false);
                    break;
                case "RobEnforcer":
                    break;
                case "HANDOverclocked":
                    GameObject.Find("HANDTeaser").SetActive(false);
                    break;
            }
            return gameObject;
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
    }
}
