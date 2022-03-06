using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.CompilerServices;
using RoR2.UI;
using UnityEngine.UI;
using BepInEx.Logging;
using System.Linq;
using RoR2.UI.SkinControllers;
using System;
using System.Collections;
using UnityEngine.EventSystems;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace BanditItemlet
{
    [BepInPlugin("com.DestroyedClone.DDSS", "Darkest Dungeon Survivor Select", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
            //{ new string[]{ "", "", "", "" }, "" },
        public static Dictionary<string[], string> characterNames_to_teamName = new Dictionary<string[], string>()
        {
            // Todo: Organize
            // Category: Rank 4
            #region CaptainBody
            { new string[]{ "CaptainBody", "EngineerBody", "PaladinBody", "EnforcerBody" }, "Protectors" },
            #endregion

            #region CommandoBody
            { new string[]{ "CommandoBody", "CommandoBody", "CommandoBody", "CommandoBody" }, "Immeasurable Newcomers" },
            #endregion

            #region CrocoBody
            { new string[]{ "CrocoBody", "Bandit2Body", "LoaderBody", "MercernaryBody" }, "Sliced Club" },
            #endregion

            #region Bandit2Body
            { new string[]{ "Bandit2Body", "CommandoBody", "EnforcerBody", "LoaderBody" }, "The Unusual Suspects" },
            { new string[]{ "Bandit2Body", "HuntressBody", "LoaderBody", "MercernaryBody" }, "Reformed Crew" },
            #endregion

            #region CrocoBody
            #endregion

            // CommandoBody
            { new string[]{ "RailgunnerBody", "SniperClassicBody", "HuntressBody", "ToolbotBody" }, "Camping Cohorts" },
            { new string[]{ "SniperClassicBody", "MinerModBody", "CHEFBody", "EnforcerBody" }, "Abandoned Adventurers" },
            { new string[]{ "MageBody", "MageBody", "MageBody", "MageBody" }, "" },
        };

        public static Dictionary<BodyIndex[], string> bodyIndices_to_teamName = new Dictionary<BodyIndex[], string>();

        public static bool compat_LobbyAppearanceImprovements = false;

        public void Start()
        {
            On.RoR2.UI.CharacterSelectController.Start += CharacterSelectController_Start;
            On.RoR2.UI.CharacterSelectController.SelectSurvivor += CharacterSelectController_SelectSurvivor;
            //On.RoR2.UI.CharacterSelectController.Update += CharacterSelectController_Update; //debugging
            CheckModCompat();
        }

        public void CheckModCompat()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.LobbyAppearanceImprovements"))
            {
                compat_LobbyAppearanceImprovements = true;
            }
        }

        private void CharacterSelectController_Update(On.RoR2.UI.CharacterSelectController.orig_Update orig, CharacterSelectController self)
        {
            GameObject firstPrefab = null;
            self.SetEventSystem(self.eventSystemLocator.eventSystem);
            if (self.previousSurvivorIndex != self.selectedSurvivorIndex)
            {
                self.RebuildLocal();
                self.previousSurvivorIndex = self.selectedSurvivorIndex;
            }
            self.UpdateSurvivorInfoPanel();
            if (self.characterDisplayPads.Length != 0)
            {
                List<NetworkUser> sortedNetworkUsersList = self.GetSortedNetworkUsersList();
                for (int i = 0; i < self.characterDisplayPads.Length; i++)
                {
                    ref CharacterSelectController.CharacterPad ptr = ref self.characterDisplayPads[i];
                    NetworkUser networkUser = sortedNetworkUsersList[0];
                    if (networkUser)
                    {
                        GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(networkUser.bodyIndexPreference);
                        if (i < DelaySetup.instance.debug_characters.Length && !DelaySetup.instance.debug_characters[i].IsNullOrWhiteSpace())
                        {
                            bodyPrefab = BodyCatalog.FindBodyPrefab(DelaySetup.instance.debug_characters[i]);
                        }

                        if (!firstPrefab) firstPrefab = bodyPrefab;
                        SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                        if (survivorDef != null)
                        {
                            SurvivorDef survivorDef2 = SurvivorCatalog.GetSurvivorDef(ptr.displaySurvivorIndex);
                            bool flag = true;
                            if (survivorDef2 != null && survivorDef2.bodyPrefab == bodyPrefab)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                GameObject displayPrefab = survivorDef.displayPrefab;
                                self.ClearPadDisplay(ptr);
                                if (!displayPrefab)
                                {
                                    displayPrefab = RoR2Content.Survivors.Commando.displayPrefab;
                                }
                                if (displayPrefab)
                                {
                                    ptr.displayInstance = UnityEngine.Object.Instantiate<GameObject>(displayPrefab, ptr.padTransform.position, ptr.padTransform.rotation, ptr.padTransform);
                                    CharacterSelectSurvivorPreviewDisplayController component = ptr.displayInstance.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
                                    if (component)
                                    {
                                        //component.networkUser = networkUser;
                                    }
                                }
                                ptr.displaySurvivorIndex = survivorDef.survivorIndex;
                                //self.OnNetworkUserLoadoutChanged(networkUser);
                            }
                        }
                        else
                        {
                            self.ClearPadDisplay(ptr);
                        }
                    }
                    else
                    {
                        self.ClearPadDisplay(ptr);
                    }
                    if (!ptr.padTransform)
                    {
                        return;
                    }
                    if (self.characterDisplayPads[i].padTransform)
                    {
                        //self.characterDisplayPads[i].padTransform.gameObject.SetActive(bodyPrefab != null);
                        self.characterDisplayPads[i].padTransform.gameObject.SetActive(self.characterDisplayPads[i].displayInstance != null);
                    }
                }
            }
            if (!RoR2Application.isInSinglePlayer)
            {
                bool flag2 = self.IsClientReady();
                self.readyButton.gameObject.SetActive(!flag2);
                self.unreadyButton.gameObject.SetActive(flag2);
            }
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
        }

        private void CharacterSelectController_SelectSurvivor(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, CharacterSelectController self, SurvivorIndex survivor)
        {
            orig(self, survivor);
            if (DelaySetup.instance)
            {
                Debug.Log(3);
                DelaySetup.instance.NewSetup();
            }
        }

        private void CharacterSelectController_Start(On.RoR2.UI.CharacterSelectController.orig_Start orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            self.gameObject.AddComponent<DelaySetup>();
        }

        public class DelaySetup : MonoBehaviour
        {
            public bool debug = false;

            public float age = 0;
            public bool hasSetup = false;

            public float mp_age = 0;
            public bool mp_hasSetup = false;

            public bool btn_GetTeamName = false;
            public HGTextMeshProUGUI hgTMP;
            public CharacterSelectController characterSelectController;
            public string teamText = "Sussimaximus";
            public HGTextMeshProUGUI theTMP;

            public string[] debug_characters = new string[]
                {
                    "Bandit2Body",
                    "HuntressBody",
                    "LoaderBody",
                    "MercernaryBody"
                };

            public static DelaySetup instance;

            public void OnEnable()
            {
                if (ror2application.)
                instance = this;
                NetworkUser.onLoadoutChangedGlobal += NetworkUser_onLoadoutChangedGlobal;
                UserProfile.onLoadoutChangedGlobal += UserProfile_onLoadoutChangedGlobal;
            }

            public void OnDisable()
            {
                instance = null;
                NetworkUser.onLoadoutChangedGlobal -= NetworkUser_onLoadoutChangedGlobal;
                UserProfile.onLoadoutChangedGlobal -= UserProfile_onLoadoutChangedGlobal;
            }

            private void NetworkUser_onLoadoutChangedGlobal(NetworkUser obj)
            {
                //Debug.Log(1);
                instance.NewSetup();
            }
            private void UserProfile_onLoadoutChangedGlobal(UserProfile obj)
            {
                //Debug.Log(2);
                instance.NewSetup();
            }

            public void FixedUpdate()
            {
                if (!hasSetup)
                {
                    age += Time.fixedDeltaTime;
                    if (age > 0.75f)
                    {
                        NewSetup();
                        hasSetup = true;
                    }
                }
                if (!mp_hasSetup)
                {
                    mp_age += Time.fixedDeltaTime;
                    if (mp_age > 1f)
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

            public string GetTeamName2()
            {
                var networkUsers = characterSelectController.GetSortedNetworkUsersList();

                List<string> bodyNames = new List<string>();
                List<BodyIndex> bodyIndices = new List<BodyIndex>();
                if (!debug)
                {
                    foreach (var networkUser in networkUsers)
                    {
                        bodyIndices.Add(networkUser.NetworkbodyIndexPreference);
                        bodyNames.Add(BodyCatalog.GetBodyName(networkUser.NetworkbodyIndexPreference));
                    }
                } else
                {
                    bodyNames = debug_characters.ToList();
                    foreach (var bodyName in debug_characters)
                    {
                        bodyIndices.Add(BodyCatalog.FindBodyIndex(bodyName));
                    }
                }
                /*
                foreach (var bodyName in bodyNames)
                {
                    Debug.Log(bodyName);
                }
                foreach (var bodyIndex in bodyIndices)
                {
                    Debug.Log(bodyIndex);
                }
                */
                /*
                if (characterNames_to_teamName.TryGetValue(bodyNames.ToArray(), out string output))
                {
                    Debug.Log(output);
                }*/

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


            public void UpdateTeamText(string newText)
            {
                hgTMP.enabled = false;
                hgTMP.text = newText;
                hgTMP.enabled = true;
            }

            public string GetTeamName()
            {
                CharacterSelectController self = gameObject.GetComponent<CharacterSelectController>();
                var networkUsers = self.GetSortedNetworkUsersList();

                List<string> boys = new List<string>();
                if (!debug)
                {
                    foreach (var networkUser in networkUsers)
                    {
                        boys.Add(BodyCatalog.GetBodyName(networkUser.NetworkbodyIndexPreference));
                    }
                } else
                {
                    List<SurvivorIndex> chosenSurvivorIndices = new List<SurvivorIndex>();
                    //KingEnderbrine code
                    //var localUser = ((MPEventSystem)EventSystem.current).localUser;
                    var currentIndex = self?.selectedSurvivorIndex ?? (SurvivorIndex)EclipseRun.cvEclipseSurvivorIndex.value;
                    var survivors = SurvivorCatalog.orderedSurvivorDefs.Where(survivorDef => !survivorDef.hidden && SurvivorCatalog.SurvivorIsUnlockedOnThisClient(survivorDef.survivorIndex));
                    
                    for (int i = 0; i < 4; i++)
                    {
                        var randomIndex = survivors.ElementAt(UnityEngine.Random.Range(0, survivors.Count())).survivorIndex;
                        while (chosenSurvivorIndices.Contains(randomIndex))
                        {
                            randomIndex = survivors.ElementAt(UnityEngine.Random.Range(0, survivors.Count())).survivorIndex;
                        }
                        chosenSurvivorIndices.Add(randomIndex);
                        var randomBodyIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(randomIndex);
                        var randomBodyName = BodyCatalog.GetBodyName(randomBodyIndex);
                        boys.Add(randomBodyName);
                    }
                }
                Debug.Log("===");
                boys.Sort();
                foreach (var character in boys)
                {
                    Debug.Log(character);
                }
                if (characterNames_to_teamName.TryGetValue(boys.ToArray(), out string output))
                {
                    Debug.Log(output);
                    return output;
                } else
                {
                    return "None";
                }
            }

            public void RepositionHUD()
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
                subheaderPanel.position = new Vector3(95, 27, 100);

                RectTransform rightHandPanel = (RectTransform)characterSelectController.transform.Find("SafeArea/RightHandPanel");
                //rightHandPanel.position = new Vector3(0f, 63f, 100f);
                rightHandPanel.rotation = Quaternion.identity;
                rightHandPanel.localScale = Vector3.one * 0.8f;
                Transform ruleVerticalLayout = rightHandPanel.Find("RuleVerticalLayout");
                ruleVerticalLayout.position = new Vector3(0f, 23f, 100f);
                ruleVerticalLayout.Find("BlurPanel").gameObject.SetActive(false);
                ruleVerticalLayout.Find("BorderImage").gameObject.SetActive(false);

                var ruleVerticalLayoutVertical = ruleVerticalLayout.Find("RuleBookViewerVertical");
                ruleVerticalLayoutVertical.GetComponent<Image>().enabled = false;
                ruleVerticalLayoutVertical.Find("Viewport/Content/RulebookCategoryPrefab(Clone)/Header").gameObject.SetActive(false);

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


                if (!compat_LobbyAppearanceImprovements)
                {
                    leftHandPanel.Find("BlurPanel").gameObject.SetActive(false);
                }

                Destroy(survivorNamePanelClone.gameObject);
            }

            public void RepositionCharacterPads()
            {
                var characterPadAlignments = GameObject.Find("CharacterPadAlignments");
                characterPadAlignments.transform.rotation = Quaternion.Euler(new Vector3(0, 195, 0));
                float index = 0;
                float xoffset = 1f;
                float zoffset = -0.25f;
                foreach (Transform child in characterPadAlignments.transform)
                {
                    child.localPosition = new Vector3(index*xoffset, 0, index*zoffset);
                    index++;
                }
            }

            public void NewSetup()
            {
                characterSelectController = gameObject.GetComponent<CharacterSelectController>();
                RepositionHUD();
                RepositionCharacterPads();
                UpdateTeamText(GetTeamName2());
            }

            public void Setup()
            {
                CharacterSelectController self = gameObject.GetComponent<CharacterSelectController>();

                //self.transform.Find("SafeArea/RightHandPanel").gameObject.SetActive(false);
                Transform leftHandPanel = self.transform.Find("SafeArea/LeftHandPanel (Layer: Main)");
                leftHandPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
                leftHandPanel.Find("BorderImage").gameObject.SetActive(false);
                leftHandPanel.eulerAngles = Vector3.zero;
                Transform survivorChoiceGrid = leftHandPanel.Find("SurvivorChoiceGrid, Panel");
                survivorChoiceGrid.eulerAngles = Vector3.zero;
                survivorChoiceGrid.position = new Vector3(0f, -45f, 100f);
                Transform survivorInfoPanel = leftHandPanel.Find("SurvivorInfoPanel, Active (Layer: Secondary)");
                survivorInfoPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
                survivorInfoPanel.transform.position = new Vector3(30, 32.14879f, 100);
                //var snp = sip.Find("SurvivorNamePanel");
                //var shp = snp.Find("SubheaderPanel (Overview, Skills, Loadout)");
                //shp.eulerAngles = new Vector3(0, 0, 270);
                //shp.position = new Vector3(95, 27, 100);

                Transform rightHandPanel = self.transform.Find("SafeArea/RightHandPanel");
                rightHandPanel.Find("BlurPanel").gameObject.SetActive(false);
                rightHandPanel.Find("BorderImage").gameObject.SetActive(false);
                rightHandPanel.Find("RuleBookViewerVertical").GetComponent<Image>().enabled = false;

                int index = 0;
                foreach (Transform child in rightHandPanel.Find("/Viewport/Content"))
                {
                    if (child.name != "RulebookCategoryPrefab(Clone)")
                        continue;
                    if (index == 0)
                    {
                        child.Find("Header").gameObject.SetActive(false);
                        index++;
                    }
                    else if (index == 1)
                    {
                        child.Find("VoteResultGridContainer").gameObject.SetActive(false);
                        break;
                    }
                }
                foreach (Transform child in rightHandPanel.Find("PopoutPanelContainer"))
                {
                    if (child.name == "PopoutPanelPrefab(Clone)")
                    {
                        if (child.Find("Main/Title and Subtitle/Title Text").GetComponent<LanguageTextMeshController>().token == "RULE_HEADER_ARTIFACTS")
                        {
                            child.Find("Main/Title and Subtitle").gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }
    }
}
