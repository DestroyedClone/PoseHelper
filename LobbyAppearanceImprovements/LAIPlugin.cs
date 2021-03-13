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
using static LobbyAppearanceImprovements.StaticValues;
using static LobbyAppearanceImprovements.Methods;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace LobbyAppearanceImprovements
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class LAIPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.1";
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

        //Custom BG
        public static ConfigEntry<int> SelectedScene { get; set; }

        // SurvivorsInLobby
        public static ConfigEntry<bool> SurvivorsInLobby { get; set; }
        public static ConfigEntry<int> SelectViewMode { get; set; }
        public static ConfigEntry<bool> ReplayAnim { get; set; }
        public static ConfigEntry<bool> LivePreview { get; set; }

        public static StaticValues.LobbyViewType LobbyViewType;
        public static StaticValues.SceneType SceneType;

        public Dictionary<SurvivorIndex, float[]> characterCameraSettings = new Dictionary<SurvivorIndex, float[]>();
        public void Awake()
        {
            SetupConfig();

            CommandHelper.AddToConsoleWhenReady();

            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;
            //On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += UpdateSurvivorCache;

            switch (LobbyViewType)
            {
                case LobbyViewType.Default:
                    break;
                case LobbyViewType.Hide:
                    On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += HideOnSelected;
                    break;
                case LobbyViewType.Zoom:
                    On.RoR2.UI.CharacterSelectController.SelectSurvivor += ZoomOnSelected;
                    break;
            }
            if (ReplayAnim.Value)
            {
                On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += ReplayAnimationOnSelect;
            }
            if (DisableShaking.Value)
                On.RoR2.PreGameShakeController.Awake += SetShakerInactive;
            if (SurvivorsInLobby.Value && LivePreview.Value)
            {
                On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += UpdateCharacterPreview;
            }
        }

        private void ReplayAnimationOnSelect(On.RoR2.UI.CharacterSelectController.orig_OnNetworkUserLoadoutChanged orig, RoR2.UI.CharacterSelectController self, NetworkUser networkUser)
        {
            orig(self, networkUser);

        }


        //update character preview
        private void UpdateCharacterPreview(On.RoR2.UI.CharacterSelectController.orig_OnNetworkUserLoadoutChanged orig, RoR2.UI.CharacterSelectController self, NetworkUser networkUser)
        {
            orig(self, networkUser);
            int num = self.GetSortedNetworkUsersList().IndexOf(networkUser);
            if (num != -1)
            {
                RoR2.UI.CharacterSelectController.CharacterPad safe = HG.ArrayUtils.GetSafe<RoR2.UI.CharacterSelectController.CharacterPad>(self.characterDisplayPads, num);
                if (safe.displayInstance)
                {
                    Loadout loadout = new Loadout();
                    networkUser.networkLoadout.CopyLoadout(loadout);
                    int bodyIndexFromSurvivorIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(safe.displaySurvivorIndex);
                    int skinIndex = (int)loadout.bodyLoadoutManager.GetSkinIndex(bodyIndexFromSurvivorIndex);
                    SkinDef safe2 = HG.ArrayUtils.GetSafe<SkinDef>(BodyCatalog.GetBodySkins(bodyIndexFromSurvivorIndex), skinIndex);
                    CharacterModel componentInChildren = safe.displayInstance.GetComponentInChildren<CharacterModel>();
                    if (componentInChildren && safe2 != null)
                    {
                        safe2.Apply(componentInChildren.gameObject);
                    }
                }
            }
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
            CharacterPadScale = Config.Bind("Background", "Character Display Scale", 1f, "Resizes character displays. "); //def 1f
            SurvivorsInLobby = Config.Bind("Background", "Survivors In Lobby", true, "Shows survivors in the lobby." +
                "\nThese background survivors don't reflect the loadouts in the lobby.");

            //Custom BG
            SelectedScene = Config.Bind("Background", "Select Scene", 0, "0 = Default");

            //other
            SelectViewMode = Config.Bind("Other", "Select View Mode (Requires SurvivorsInLobby set to true)", 0, "0 = None" +
                "\n1 = Disappear on selection" +
                "\n2 = Zoom on selection"); //def 1f
            ReplayAnim = Config.Bind("Background", "Replay Animation", true, "Replays the animation for the selected character.");
            LivePreview = Config.Bind("Background", "Live Preview", true, "Updates the appearance for the selected character.");


            LobbyViewType = (StaticValues.LobbyViewType)SelectViewMode.Value;
            SceneType = (StaticValues.SceneType)SelectedScene.Value;
        }

        private void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            //var dirtycomp = self.gameObject.AddComponent<Testing.DirtyCam>();
            //dirtycomp.cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();

            var directionalLight = GameObject.Find("Directional Light");
            var ui_origin = GameObject.Find("CharacterSelectUI").transform;
            var SafeArea = ui_origin.Find("SafeArea").transform;
            var ui_left = SafeArea.Find("LeftHandPanel (Layer: Main)");
            var ui_right = SafeArea.Find("RightHandPanel");

            //Light
            if (Light_Color.Value != "default" && TryParseHtmlString(Light_Color.Value, out Color color))
                Methods.ChangeLobbyLightColor(color);
            directionalLight.gameObject.GetComponent<Light>().intensity = Light_Intensity.Value;
            directionalLight.gameObject.GetComponent<FlickerLight>().enabled = !Light_Flicker_Disable.Value;


            if (MeshProps.Value)
            {
                GameObject.Find("HANDTeaser")?.SetActive(false);
                GameObject.Find("MeshProps").SetActive(false);
                GameObject.Find("HumanCrate1Mesh").SetActive(false);
                GameObject.Find("HumanCrate2Mesh").SetActive(false);
                GameObject.Find("HumanCanister1Mesh").SetActive(false);
            }
            else if (PhysicsProps.Value)
            {
                var thing = GameObject.Find("MeshProps").transform;
                foreach (string text in new string[] { "PropAnchor", "ExtinguisherMesh", "FolderMesh", "LaptopMesh (1)", "ChairPropAnchor", "ChairMesh",
                    "ChairWeight","PropAnchor (1)","ExtinguisherMesh (1)","ExtinguisherMesh (2)", "FolderMesh (1)", "LaptopMesh (2)"})
                {
                    thing.Find(text)?.gameObject.SetActive(false);
                }
            }
            if (SurvivorsInLobby.Value)
            {
                var component = self.gameObject.AddComponent<LAI_BGCHARCOMP>();
                var characterHolder = new GameObject("HOLDER: Characters");
                var survivorDisplays = component.survivorDisplays;

                foreach (var setting in StaticValues.characterDisplaySettings)
                {
                    CreateDisplayMaster(setting.Key, setting.Value[0], setting.Value[1], characterHolder.transform, survivorDisplays);
                }
                if (LobbyViewType == StaticValues.LobbyViewType.Zoom) //here
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
                if (LobbyViewType != StaticValues.LobbyViewType.Zoom) //if Zoom is selected, then this will NRE //here
                    GameObject.Find("CharacterPadAlignments").transform.localScale *= CharacterPadScale.Value;
            }
            if (UIScale.Value != 1f)
            {
                ui_left.localScale *= UIScale.Value;
                ui_right.localScale *= UIScale.Value;
            }
            if (BlurValue.Value != 255) // default value doesnt cast well
            {
                var leftBlurColor = ui_left.Find("BlurPanel").GetComponent<TranslucentImage>().color;
                leftBlurColor.a = Mathf.Clamp(BlurValue.Value, 0f, 255f);
                var rightBlurColor = ui_right.Find("RuleVerticalLayout").Find("BlurPanel").GetComponent<TranslucentImage>().color;
                rightBlurColor.a = Mathf.Clamp(BlurValue.Value, 0f, 255f);
            }
            if (BlurValue.Value == 00030)
            {
                var SurvivorChoiceGrid = ui_left.Find("SurvivorChoiceGrid, Panel");
                ui_left.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().enabled = false;
                var KingEnderBrine = SurvivorChoiceGrid.Find("SurvivorChoiseGridContainer");
                if (KingEnderBrine)
                {
                    KingEnderBrine.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().enabled = false;
                    KingEnderBrine.transform.position = new Vector3(-70, 55, 100);
                }
                else
                {

                }
            }
        }

        // case LobbyViewType.Hide:
        private void HideOnSelected(On.RoR2.UI.CharacterSelectController.orig_OnNetworkUserLoadoutChanged orig, RoR2.UI.CharacterSelectController self, NetworkUser networkUser)
        {
            orig(self, networkUser);
            Methods.HideBackgroundCharacters(self);
        }
        private void ZoomOnSelected(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, RoR2.UI.CharacterSelectController self, SurvivorIndex survivor)
        {
            orig(self, survivor);
            var cameraRig = GameObject.Find("Main Camera").gameObject.GetComponent<CameraRigController>();
            if (characterCameraSettings.TryGetValue(survivor, out float[] cameraParams))
            {
                Methods.SetCamera(cameraRig, cameraParams[0], cameraParams[1], cameraParams[2]);
            }
            else
            {
                Methods.SetCamera(cameraRig);
            }
        }


        private void SetShakerInactive(On.RoR2.PreGameShakeController.orig_Awake orig, PreGameShakeController self)
        {
            orig(self);
            self.gameObject.SetActive(false);
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
                }
                else
                {
                    //Debug.Log("SurvivorDef was null");
                }

            }
        }





    }
}
