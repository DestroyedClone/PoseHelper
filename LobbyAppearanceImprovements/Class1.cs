using BepInEx;
using BepInEx.Configuration;
using LeTai.Asset.TranslucentImage;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using static UnityEngine.ColorUtility;

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

        public Dictionary<SurvivorIndex, float[]> characterCameraSettings = new Dictionary<SurvivorIndex, float[]>();

        public void Awake()
        {
            SetupConfig();

            CommandHelper.AddToConsoleWhenReady();

            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;

            switch (LobbyViewType)
            {
                case StaticValues.LobbyViewType.Default:
                    break;
                case StaticValues.LobbyViewType.Hide:
                    On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += HideOnSelected;
                    break;
                case StaticValues.LobbyViewType.Zoom:
                    On.RoR2.UI.CharacterSelectController.SelectSurvivor += ZoomOnSelected;
                    break;
            }
            if (DisableShaking.Value)
                On.RoR2.PreGameShakeController.Awake += PreGameShakeController_Awake;
        }

        public void SetupConfig()
        {
            //default new Color32((byte)0.981, (byte)0.356, (byte)0.356, (byte)1.000)
            //250.155, 90.78, 90.78
            // Lights
            Light_Color = Config.Bind("Lights", "Hex Color", "default", "Change the default color of the light"); //#fa5a5a
            Light_Flicker_Disable = Config.Bind("Lights", "Disable FlickerLight", true, "Makes the light not flicker anymore.");
            Light_Intensity = Config.Bind("Lights", "Intensity", 1f, "Change the intensity of the light.");

            //UI
            PostProcessing = Config.Bind("UI", "Disable Post Processing", true, "Disables the blurry post processing.");
            HideFade = Config.Bind("UI", "Hide Fade", true, "There's a dark fade on the top and bottom, this disables it.");
            BlurValue = Config.Bind("UI", "Adjust Blur (Not Implemented)", 255, "Adjusts the blur behind the UI elements on the left and right." +
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
        }
        private void PreGameShakeController_Awake(On.RoR2.PreGameShakeController.orig_Awake orig, PreGameShakeController self)
        {
            orig(self);
            self.gameObject.SetActive(false);
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
            var dirtycomp = self.gameObject.AddComponent<Testing.DirtyCam>();
            dirtycomp.cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();

            var directionalLight = GameObject.Find("Directional Light");
            var ui_origin = GameObject.Find("CharacterSelectUI").transform;
            var SafeArea = ui_origin.Find("SafeArea").transform;
            var ui_left = SafeArea.Find("LeftHandPanel (Layer: Main)");
            var ui_right = SafeArea.Find("RightHandPanel");

            //Light
            if (Light_Color.Value != "default" && TryParseHtmlString(Light_Color.Value, out Color color))
                Helpers.ChangeLobbyLightColor(color);
            directionalLight.gameObject.GetComponent<Light>().intensity = Light_Intensity.Value;
            if (Light_Flicker_Disable.Value)
            {
                directionalLight.gameObject.GetComponent<FlickerLight>().enabled = false;
            }


            if (MeshProps.Value)
            {
                GameObject.Find("HANDTeaser")?.SetActive(false);
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
                if (LobbyViewType == StaticValues.LobbyViewType.Zoom)
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
                if (LobbyViewType != StaticValues.LobbyViewType.Zoom) //if Zoom is selected, then this will NRE
                    GameObject.Find("CharacterPadAlignments").transform.localScale *= CharacterPadScale.Value;
            }
            if (UIScale.Value != 1f)
            {
                ui_left.localScale *= UIScale.Value;
                ui_right.localScale *= UIScale.Value;
                //rtSide.position = new Vector3(80, 30, 90);
            }
            if (BlurValue.Value != 255) // default value doesnt cast well
            {
                var leftBlurColor = ui_left.Find("BlurPanel").GetComponent<TranslucentImage>().color;
                leftBlurColor.a = Mathf.Clamp(BlurValue.Value, 0f, 255f);
                var rightBlurColor = ui_right.Find("RuleVerticalLayout").Find("BlurPanel").GetComponent<TranslucentImage>().color;
                rightBlurColor.a = Mathf.Clamp(BlurValue.Value, 0f, 255f);
            }
            //if (true == false)
            //{
                //var SurvivorChoiceGrid = ui_left.Find("SurvivorChoiceGrid, Panel");
               // ui_left.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().enabled = false;
                //var KingEnderBrine = SurvivorChoiceGrid.Find("SurvivorChoiseGridContainer");
                //if (KingEnderBrine)
                //{
                    //KingEnderBrine.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().enabled = false;
                    //KingEnderBrine.transform.position = new Vector3(-70, 55, 100);
                //} else
                //{

                //}
            //}
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
            //Debug.Log("Attempting to get body prefab from "+bodyPrefabName);
            var bodyPrefab = GetBodyPrefab(bodyPrefabName);
            if (bodyPrefab)
            {
                //Debug.Log("Getting survivor def");
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                if (survivorDef != null)
                {
                    //Debug.Log("SurvivorDef wasn't null");
                    //Debug.Log("Getting survivor index");
                    SurvivorIndex survivorIndex = survivorDef.survivorIndex;
                    //Debug.Log("Attempting " + bodyPrefab + "for index " + survivorIndex);
                    if (survivorIndex >= 0) //invalid values are -1
                    {
                        //Debug.Log("Works!");
                        if (!keyValuePairs.ContainsKey(survivorDef.survivorIndex))
                        {
                            var display = CreateDisplay(bodyPrefabName, position, rotation, parent);
                            keyValuePairs.Add(survivorDef.survivorIndex, display);
                        }
                        if (!characterCameraSettings.ContainsKey(survivorIndex))
                        {
                            StaticValues.textCameraSettings.TryGetValue(bodyPrefabName, out float[] cameraSetting);
                            characterCameraSettings.Add(survivorIndex, cameraSetting);
                        }
                    }
                    else
                    {
                        //Debug.Log("Doesnt!");
                    }
                } else
                {
                    //Debug.Log("SurvivorDef was null");
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
            if (!bodyPrefab) return null;
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


    public static class Helpers
    {
        public static void ChangeLobbyLightColor(Color32 color)
        {
            GameObject.Find("Directional Light").gameObject.GetComponent<Light>().color = color;
        }
    }
}
