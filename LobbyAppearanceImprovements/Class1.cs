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
using LeTai.Asset.TranslucentImage;

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
        //Lights
        public static ConfigEntry<string> Light_Color { get; set; }
        public static ConfigEntry<bool> Light_Flicker_Disable { get; set; }
        public static ConfigEntry<float> Light_Intensity { get; set; }

        //UI
        public static ConfigEntry<bool> PostProcessing { get; set; }
        public static ConfigEntry<bool> HideFade { get; set; }
        public static ConfigEntry<int> BlurValue { get; set; }
        public static ConfigEntry<float> UIScale { get; set; }

        //BG
        public static ConfigEntry<float> CharacterPadScale { get; set; }
        public static ConfigEntry<bool> MeshProps { get; set; }
        public static ConfigEntry<bool> PhysicsProps { get; set; }
        public static ConfigEntry<bool> DisableShaking { get; set; }
        public static ConfigEntry<bool> SurvivorsInLobby { get; set; }
        public static ConfigEntry<int> SelectViewMode { get; set; }
        public static StaticValues.LobbyViewType LobbyViewType;

        //Other
        public static ConfigEntry<bool> DevMode { get; set; }

        public Dictionary<SurvivorIndex, float[]> characterCameraSettings = new Dictionary<SurvivorIndex, float[]>();

        public void Awake()
        {
            //default new Color32((byte)0.981, (byte)0.356, (byte)0.356, (byte)1.000)
            //250.155, 90.78, 90.78
            // Lights
            Light_Color = Config.Bind("Lights", "Hex Color", "#fa5a5a", "Change the default color of the light");
            Light_Flicker_Disable = Config.Bind("Lights", "Disable FlickerLight", true, "Makes the light not flicker anymore.");
            Light_Intensity = Config.Bind("Lights", "Intensity", 1f, "Change the intensity of the light.");

            //UI
            PostProcessing = Config.Bind("UI", "Disable Post Processing", true, "Disables the blurry post processing.");
            HideFade = Config.Bind("UI", "Hide Fade", true, "There's a dark fade on the top and bottom, this disables it.");
            BlurValue = Config.Bind("UI", "Adjust Blur", 255, "Adjusts the blur behind the UI elements on the left and right." +
                "\n0:fully transparent - 255:default");
            UIScale = Config.Bind("UI", "UI Scale", 1f, "Resizes the UIs on the left and right."); //def 1f

            //BG
            MeshProps = Config.Bind("Background", "Hide MeshProps", false, "Hides all the background meshprops.");
            PhysicsProps = Config.Bind("Background", "Hide Physics Props", false, "Hides only the physics props like the Chair.");
            DisableShaking = Config.Bind("Background", "Disable Shaking", false, "Disables the random shaking that rattles the ship.");
            SurvivorsInLobby = Config.Bind("Background", "Survivors In Lobby", true, "Shows survivors in the lobby." +
                "\nThese background survivors don't reflect the loadouts in the lobby.");
            CharacterPadScale = Config.Bind("Background", "Character Display Scale", 1f, "Resizes character displays. "); //def 1f

            //other
            SelectViewMode = Config.Bind("Other", "Select View Mode (Requires SurvivorsInLobby set to true)", 0, "0 = None" +
                "\n1 = Disappear on selection" +
                "\n2 = Zoom on selection"); //def 1f
            LobbyViewType = (StaticValues.LobbyViewType)SelectViewMode.Value;
            DevMode = Config.Bind("Other", "Enable Dev Stuff", false, "Really only needed if you wanted to fine-tune settings or for the readme.");

            CommandHelper.AddToConsoleWhenReady();

            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;

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
            //var dirtycomp = self.gameObject.AddComponent<DirtyCam>();
            //dirtycomp.cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();
            //var tweenController = self.gameObject.AddComponent<CameraTweenController>();
            //tweenController.cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();

            var directionalLight = GameObject.Find("Directional Light");
            var ui_origin = GameObject.Find("CharacterSelectUI").transform;
            var ui = ui_origin.Find("SafeArea").transform;
            var ui_left = ui.Find("LeftHandPanel (Layer: Main)");
            var ui_right = ui.Find("RightHandPanel");

            //Light
            if (TryParseHtmlString(Light_Color.Value, out Color color))
                Helpers.ChangeLobbyLightColor(color);
            directionalLight.gameObject.GetComponent<Light>().intensity = Light_Intensity.Value;
            if (Light_Flicker_Disable.Value)
            {
                directionalLight.gameObject.GetComponent<FlickerLight>().enabled = false;
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
                if (PhysicsProps.Value)
                {
                    var thing = GameObject.Find("MeshProps").transform;
                    foreach (string text in new string[] { "PropAnchor", "ExtinguisherMesh", "FolderMesh", "LaptopMesh (1)", "ChairPropAnchor", "ChairMesh",
                    "ChairWeight","PropAnchor (1)","ExtinguisherMesh (1)","ExtinguisherMesh (2)", "FolderMesh (1)", "LaptopMesh (2)"})
                    {
                        thing.Find(text)?.gameObject.SetActive(false);
                    }
                }
            }
            if (SurvivorsInLobby.Value)
            {
                var component = self.gameObject.AddComponent<BackgroundCharacterDisplayToggler>();
                var characterHolder = new GameObject("HOLDER: Characters");
                var dict = component.survivorDisplays;

                foreach (var setting in StaticValues.characterDisplaySettings)
                {
                    CreateDisplayMaster(setting.Key, setting.Value[0], setting.Value[1], characterHolder.transform, dict);
                }
                if (SelectViewMode.Value > 1)
                {
                    GameObject.Find("CharacterPadAlignments").SetActive(false);
                }
            }
            
            if (PostProcessing.Value)
            {
                GameObject.Find("PP").SetActive(false);
            }
            if (HideFade.Value)
            {
                ui_origin.Find("BottomSideFade").gameObject.SetActive(false);
                ui_origin.Find("TopSideFade").gameObject.SetActive(false);
            }
            if (CharacterPadScale.Value != 1f)
            {
                if (SelectViewMode.Value <= 1) //if mode 2 is selected, then this will NRE
                    GameObject.Find("CharacterPadAlignments").transform.localScale *= CharacterPadScale.Value;
            }
            if (UIScale.Value != 1f)
            {
                ui_left.transform.localScale *= UIScale.Value;
                ui_right.localScale *= UIScale.Value;
                //rtSide.position = new Vector3(80, 30, 90);
            }
            if (BlurValue.Value != (int)BlurValue.DefaultValue)
            {
                var leftBlurColor = ui_left.Find("BlurPanel").GetComponent<TranslucentImage>().color;
                leftBlurColor.a = Mathf.Clamp(BlurValue.Value, 0f, 255f);
                var rightBlurColor = ui_right.Find("RuleVerticalLayout").Find("BlurPanel").GetComponent<TranslucentImage>().color;
                rightBlurColor.a = Mathf.Clamp(BlurValue.Value, 0f, 255f);
            }
            if (DisableShaking.Value)
            {
                GameObject.Find("PreGameController").transform.Find("PreGameShake").gameObject.SetActive(false);
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
                // Now we can disable
                foreach (var currentDisplays in self.characterDisplayPads)
                {
                    var index = currentDisplays.displaySurvivorIndex;
                    component.survivorDisplays.TryGetValue(index, out GameObject objectToToggle);
                    objectToToggle.SetActive(false);
                    //selectedCharacters.Add(currentDisplays.displaySurvivorIndex);
                }
            }
        }

        public class BackgroundCharacterDisplayToggler: MonoBehaviour
        {
            public Dictionary<SurvivorIndex, GameObject> survivorDisplays = new Dictionary<SurvivorIndex, GameObject>();
        }
        public void CreateDisplayMaster(string bodyPrefabName, Vector3 position, Vector3 rotation, Transform parent = null, Dictionary<SurvivorIndex, GameObject> keyValuePairs = null)
        {
            var bodyPrefab = GetBodyPrefab(bodyPrefabName);
            if (bodyPrefab)
            {
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                if (!keyValuePairs.ContainsKey(survivorDef.survivorIndex))
                {
                    var display = CreateDisplay(bodyPrefabName, position, rotation, parent);
                    keyValuePairs.Add(survivorDef.survivorIndex, display);
                }

                SurvivorIndex survivorIndex = survivorDef.survivorIndex;
                if (!characterCameraSettings.ContainsKey(survivorIndex))
                {
                    StaticValues.textCameraSettings.TryGetValue(bodyPrefabName, out float[] cameraSetting);
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
