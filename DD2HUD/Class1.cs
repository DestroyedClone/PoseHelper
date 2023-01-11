using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using static DD2HUD.Assets;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace DD2HUD
{
    [BepInPlugin("com.DestroyedClone.DD2Lobby", "Darkest Dungeon 2 Lobby", "1.0.0")]
    //[BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    //[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Main : BaseUnityPlugin
    {
        public static Dictionary<BodyIndex[], string> bodyIndices_to_teamName = new Dictionary<BodyIndex[], string>();

        private readonly bool DEBUG_addfakenetworkusers = false;

        public void Start()
        {
            if (DEBUG_addfakenetworkusers)
            {
                Logger.LogWarning("Debug mode is on, disable before compiling and uploading!");
                DD2LobbySetupComponent.debug = DEBUG_addfakenetworkusers;
            }
            SetupConfig();
            ModCompatibility.CheckModCompatibility();
            On.RoR2.UI.CharacterSelectController.Start += CharacterSelectController_Start;
            On.RoR2.UI.CharacterSelectController.SelectSurvivor += CharacterSelectController_SelectSurvivor;
            if (DEBUG_addfakenetworkusers)
                On.RoR2.UI.CharacterSelectController.OnDisable += CharacterSelectController_OnDisable; //debugging


            //R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        public void SetupConfig()
        {
            cfgModifyCharacterPosition = Config.Bind("", "Modify Character Display Positions", true, "If true, then the character positions will be modified in the lobby.");
        }


        private void CharacterSelectController_Start(On.RoR2.UI.CharacterSelectController.orig_Start orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            self.gameObject.AddComponent<DD2LobbySetupComponent>();
        }

        private void CharacterSelectController_SelectSurvivor(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, CharacterSelectController self, SurvivorIndex survivor)
        {
            orig(self, survivor);
            if (DD2LobbySetupComponent.instance)
            {
                DD2LobbySetupComponent.instance.UpdateTeamName(survivor);
            }
        }

        private void CharacterSelectController_OnDisable(On.RoR2.UI.CharacterSelectController.orig_OnDisable orig, CharacterSelectController self)
        {
            orig(self);
            var netUsers = InstanceTracker.GetInstancesList<FakeNetworkUserMarker>();
            foreach (var user in netUsers.ToList())
            {
                Destroy(user.gameObject);
            }
        }

        [ConCommand(commandName = "dd2lobby_debug_setNetworkBodies", flags = ConVarFlags.None, helpText = "Dev only Command. Rank 1, Rank2, Rank 3, Rank 4. Rank 1 is ignored as its your playerslot")]
        private static void CCChangeNetworkBodies(ConCommandArgs args)
        {
            DD2LobbySetupComponent.debug_characters = new string[]
            {
                args.GetArgString(0),
                args.GetArgString(1),
                args.GetArgString(2),
                args.GetArgString(3)
            };
        }

        [ConCommand(commandName = "dd2lobby_debug_toggle", flags = ConVarFlags.None, helpText = "Dev only Command. true or false")]
        private static void CCClearChat(ConCommandArgs args)
        {
            DD2LobbySetupComponent.debug = args.GetArgBool(0);
            Debug.Log($"dd2lobby_debug_toggle set to {args.GetArgBool(0)}");
        }

        [RoR2.SystemInitializer(dependencies: new System.Type[]
        {
            typeof(BodyCatalog),
            typeof(RoR2.SurvivorCatalog),
            typeof(RoR2.MasterCatalog),
        })]
        public static void ConvertBodyNamesToBodyIndices()
        {
            foreach (var entry in characterNames_to_teamName)
            {
                List<BodyIndex> bodyIndices = new List<BodyIndex>();
                foreach (var bodyName in entry.Key)
                {
                    bodyIndices.Add(BodyCatalog.FindBodyIndex(bodyName));
                }
                bodyIndices_to_teamName.Add(bodyIndices.ToArray(), entry.Value);
            }

            var index = 0;
            foreach (var a in bodyIndices_to_teamName)
            {
                var text = $"{index} : ";
                foreach (var b in a.Key)
                {
                    text += $"{b}, ";
                }
                text += $"{a.Value}";
                Debug.Log(text);
            }
            Debug.Log($"Team Count: {bodyIndices_to_teamName.Count}");
        }

        public class FakeNetworkUserMarker : MonoBehaviour
        {
            public void OnEnable()
            { InstanceTracker.Add(this); }

            public void OnDisable()
            { InstanceTracker.Remove(this); Debug.Log("Destroyed Fake User " + name); }
        }

        public class DD2LobbySetupComponent : MonoBehaviour
        {
            public float age = 0;
            public bool hasSetup = false;

            public float mp_age = 0;
            public bool mp_hasSetup = false;

            public static bool debug = false;
            public bool btn_GetTeamName = false;
            public HGTextMeshProUGUI hgTMP;
            public CharacterSelectController characterSelectController;

            //public string teamText = "Sussimaximus";
            public HGTextMeshProUGUI theTMP;

            public static string[] debug_characters = new string[]
                {
                    "Bandit2Body",
                    "HuntressBody",
                    "LoaderBody",
                    "MercBody"
                };

            public static DD2LobbySetupComponent instance;

            private void OnEnable()
            {
                mp_hasSetup = RoR2Application.isInSinglePlayer;
                instance = this;
                NetworkUser.onLoadoutChangedGlobal += NetworkUser_onLoadoutChangedGlobal;
                UserProfile.onLoadoutChangedGlobal += UserProfile_onLoadoutChangedGlobal;
            }

            private void OnDisable()
            {
                instance = null;
                NetworkUser.onLoadoutChangedGlobal -= NetworkUser_onLoadoutChangedGlobal;
                UserProfile.onLoadoutChangedGlobal -= UserProfile_onLoadoutChangedGlobal;
            }

            private void NetworkUser_onLoadoutChangedGlobal(NetworkUser obj)
            { instance.NewSetup(); }

            private void UserProfile_onLoadoutChangedGlobal(UserProfile obj)
            { instance.NewSetup(); }

            public ulong GetNetworkName(string bodyName)
            {
                switch (bodyName)
                {
                    case "CrocoBody": return 76561198124912729;
                    case "MageBody": return 76561198348262420;
                    case "Bandit2Body": return 76561198064142548;
                    case "Captain": return 76561198177832603;
                    case "CommandoBody": return 76561198115125395;
                    case "EngineerBody": return 76561197988953445;
                    case "HuntressBody": return 76561198210922492;
                    case "LoaderBody": return 76561198246537066;
                    case "MercBody": return 76561198198967470;
                    case "ToolbotBody": return 76561198069591654;
                    case "TreebotBody": return 76561198267390855;
                    default: return 76561197960447933;
                }
            }

            public void CreateTemporaryNetworkUsers()
            {
                if (!debug) return;
                GameObject meUser = LocalUserManager.GetFirstLocalUser().currentNetworkUser.gameObject;
                List<string> bodyNamesToCopy = new List<string>(DD2LobbySetupComponent.debug_characters);
                bodyNamesToCopy.RemoveAt(0);
                foreach (string bodyName in bodyNamesToCopy)
                {
                    if (InstanceTracker.GetInstancesList<FakeNetworkUserMarker>().Count == 3)
                        break;
                    var copy = Instantiate(meUser);
                    var nU = copy.GetComponent<NetworkUser>();
                    nU.SetBodyPreference(BodyCatalog.FindBodyIndex(bodyName));
                    nU.id = new NetworkUserId()
                    {
                        value = GetNetworkName(bodyName)
                    };
                    copy.name = bodyName;
                    copy.AddComponent<FakeNetworkUserMarker>();
                    Debug.Log($"Adding fake NetworkUser as {bodyName}");
                }
            }

            private void FixedUpdate()
            {
                if (!hasSetup)
                {
                    age += Time.fixedDeltaTime;
                    if (age > 0.35f)
                    {
                        CreateTemporaryNetworkUsers();
                        NewSetup();
                        hasSetup = true;
                    }
                }
                if (!mp_hasSetup)
                {
                    mp_age += Time.fixedDeltaTime;
                    if (age > 0.75f)
                    {
                        NewSetup();
                        mp_hasSetup = true;
                    }
                }
                if (btn_GetTeamName)
                {
                    //GetTeamName();
                    btn_GetTeamName = false;
                }
            }

            private string GetTeamName2(SurvivorIndex firstSurvivorIndex = SurvivorIndex.None)
            {
                var networkUsers = characterSelectController.GetSortedNetworkUsersList();

                List<string> bodyNames = new List<string>();
                List<BodyIndex> bodyIndices = new List<BodyIndex>();
                if (!debug)
                {
                    if (networkUsers.Count <= 3)
                    {
                        return string.Empty;
                    }
                    foreach (var networkUser in networkUsers)
                    {
                        bodyIndices.Add(networkUser.NetworkbodyIndexPreference);
                        bodyNames.Add(BodyCatalog.GetBodyName(networkUser.NetworkbodyIndexPreference));
                    }
                }
                else
                {
                    bodyNames = debug_characters.ToList();
                    foreach (var bodyName in debug_characters)
                    {
                        bodyIndices.Add(BodyCatalog.FindBodyIndex(bodyName));
                    }
                }
                if (firstSurvivorIndex != SurvivorIndex.None)
                {
                    bodyIndices[0] = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(firstSurvivorIndex);
                    bodyNames[0] = SurvivorCatalog.GetSurvivorDef(firstSurvivorIndex).bodyPrefab.name;
                }

                /*
                if (characterNames_to_teamName.TryGetValue(bodyNames.ToArray(), out string output))
                {
                    Debug.Log(output);
                }*/

                foreach (var name in bodyNames)
                {
                    Debug.Log(name);
                }

                foreach (var entry in characterNames_to_teamName)
                {
                    var key = entry.Key;
                    if (
                        key[0] == bodyNames[0] &&
                        key[1] == bodyNames[1] &&
                        key[2] == bodyNames[2] &&
                        key[3] == bodyNames[3]
                        )
                    {
                        return entry.Value;
                    }
                }
                return string.Empty;
            }

            private void UpdateTeamText(string newText)
            {
                hgTMP.enabled = false;
                hgTMP.text = newText;
                hgTMP.enabled = true;
            }

            //public Transform difficultySection;

            private void RepositionHUD()
            {
                Transform leftHandPanel = characterSelectController.transform.Find("SafeArea/LeftHandPanel (Layer: Main)");
                leftHandPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
                leftHandPanel.Find("BorderImage").gameObject.SetActive(false);
                leftHandPanel.eulerAngles = Vector3.zero;
                Transform survivorChoiceGrid = leftHandPanel.Find("SurvivorChoiceGrid, Panel");
                survivorChoiceGrid.eulerAngles = Vector3.zero;
                survivorChoiceGrid.position = new Vector3(0f, -45f, 100f);
                Transform survivorInfoPanel = leftHandPanel.Find("SurvivorInfoPanel, Active (Layer: Secondary)");
                survivorInfoPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
                survivorInfoPanel.transform.position = new Vector3(30, 32.14879f, 100);
                Transform survivorNamePanel = survivorInfoPanel.Find("SurvivorNamePanel");
                Transform survivorNamePanelClone = null;
                if (!theTMP || !hgTMP)
                    survivorNamePanelClone = Instantiate(survivorNamePanel, characterSelectController.transform);
                survivorInfoPanel.Find("ContentPanel (Overview, Skills, Loadout)/SkillScrollContainer/DescriptionPanel, Skill").position =
                    new Vector3(-50, 19.66f, 100);

                if (!theTMP)
                {
                    Transform theText = UnityEngine.Object.Instantiate(survivorNamePanelClone, survivorNamePanel);
                    theText.localPosition = new Vector3(70, 20, 0);
                    theText.name = "TheText";
                    theTMP = theText.Find("SurvivorName").GetComponent<HGTextMeshProUGUI>();
                    theTMP.text = "The";
                    theTMP.transform.localPosition = Vector3.zero;
                }

                Transform subheaderPanel = survivorInfoPanel.Find("SubheaderPanel (Overview, Skills, Loadout)");
                subheaderPanel.eulerAngles = new Vector3(0, 0, 270);
                subheaderPanel.position = new Vector3(100, 27, 100);

                RectTransform rightHandPanel = (RectTransform)characterSelectController.transform.Find("SafeArea/RightHandPanel");
                //rightHandPanel.position = new Vector3(0f, 63f, 100f);
                rightHandPanel.rotation = Quaternion.identity;
                rightHandPanel.localScale = Vector3.one * 0.8f;
                //rightHandPanel.position = new Vector3(20, -20, 100);
                Transform ruleVerticalLayout = rightHandPanel.Find("RuleVerticalLayout");
                ruleVerticalLayout.position = new Vector3(-75, 20, 100); //new Vector3(0f, 23f, 100f);
                ruleVerticalLayout.Find("BlurPanel").gameObject.SetActive(false);
                ruleVerticalLayout.Find("BorderImage").gameObject.SetActive(false);

                var ruleVerticalLayoutVertical = ruleVerticalLayout.Find("RuleBookViewerVertical");
                ruleVerticalLayoutVertical.GetComponent<Image>().enabled = false;
                //var difficultySection = ruleVerticalLayoutVertical.Find("Viewport/Content/RulebookCategoryPrefab(Clone)");
                //difficultySection.Find("/Header").gameObject.SetActive(false);
                ruleVerticalLayoutVertical.Find("Viewport/Content/RulebookCategoryPrefab(Clone)/Header").gameObject.SetActive(false);
                //difficultySection.parent = difficultySection.parent.parent; //(needs permanent reference)

                Transform readyPanel = characterSelectController.transform.Find("SafeArea/ReadyPanel");
                readyPanel.position = new Vector3(80, -45, 100);
                if (!hgTMP)
                {
                    Transform teamText = UnityEngine.Object.Instantiate(survivorNamePanelClone, readyPanel.Find("ReadyButton"));
                    teamText.localPosition = new Vector3(-730, 20, 0);
                    teamText.name = "TeamText";
                    hgTMP = teamText.Find("SurvivorName").GetComponent<HGTextMeshProUGUI>();
                }

                var survivorTMP = survivorNamePanel.Find("SurvivorName").GetComponent<HGTextMeshProUGUI>();
                survivorTMP.transform.localScale = Vector3.one * 2f;

                Transform chatBox = characterSelectController.transform.Find("SafeArea/ChatboxPanel");
                chatBox.localPosition = new Vector3(-700, 305, 0);

                if (!compat_LobbyAppearanceImprovements)
                {
                    leftHandPanel.Find("BlurPanel").gameObject.SetActive(false);
                }

                if (survivorNamePanelClone)
                    Destroy(survivorNamePanelClone.gameObject);
            }

            private void RepositionCharacterPads()
            {
                if (!cfgModifyCharacterPosition.Value) return;
                if (!debug && characterSelectController.GetSortedNetworkUsersList().Count <= 1)
                {
                    return;
                }
                var characterPadAlignments = GameObject.Find("CharacterPadAlignments");
                characterPadAlignments.transform.rotation = Quaternion.Euler(new Vector3(0, 195, 0));

                float index = 0;
                float xoffset = 1f;
                float zoffset = -0.25f;
                foreach (Transform child in characterPadAlignments.transform)
                {
                    child.localPosition = new Vector3(index * xoffset, 0, index * zoffset);
                    index++;
                }
            }

            public void NewSetup(SurvivorIndex survivorIndex = SurvivorIndex.None)
            {
                characterSelectController = gameObject.GetComponent<CharacterSelectController>();
                RepositionHUD();
                RepositionCharacterPads();
                UpdateTeamName(survivorIndex);
            }

            public void UpdateTeamName(SurvivorIndex survivorIndex = SurvivorIndex.None)
            {
                UpdateTeamText(GetTeamName2(survivorIndex));
            }
        }
    }

    public class ModCompatibility
    {
        public static bool compat_LobbyAppearanceImprovements = false;

        public static void CheckModCompatibility()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.LobbyAppearanceImprovements"))
            {
                compat_LobbyAppearanceImprovements = true;
            }
        }
    }

    public static class Config
    {
        public static ConfigEntry<bool> cfgModifyCharacterPosition;
    }
}