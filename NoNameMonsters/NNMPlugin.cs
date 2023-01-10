using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Security;
using System.Text;
using BepInEx.Logging;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace UnidentifiedLanding
{
    [BepInPlugin("com.DestroyedClone.UnidentifiedLanding", "Unidentified Landing", "1.0.0")]
    public class UnidentifiedLandingPlugin : BaseUnityPlugin
    {
        //for identification
        //private static readonly Dictionary<string, string> nameToken_to_prefabName = new Dictionary<string, string>();
        private static readonly Dictionary<string, UnlockableDef> nameToken_to_unlockableDef = new Dictionary<string, UnlockableDef>();

        /*
         * 
        */
        private static readonly Dictionary<string, string> replacementTokenDict = new Dictionary<string, string>();
        public static ConfigFile _config;
        public static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _config = Config;
            _logger = Logger;
        }


        [RoR2.SystemInitializer(dependencies: new Type[] { 
            typeof(BodyCatalog),
            typeof(SurvivorCatalog),
            typeof(ItemCatalog),
            typeof(EquipmentCatalog),
            typeof(SceneCatalog)})]
        public static void AssignDictionary()
        {
            bool IsValid(string token)
            {
                return !token.IsNullOrWhiteSpace()
                    && !nameToken_to_unlockableDef.ContainsKey(token);
            }
            /*
            bool assignMain(string category, string defaultReplacementValue, out string replacementToken)
            {
                replacementToken = _config.Bind<string>(category, "Replacement Token", defaultReplacementValue, "The new token that the original value will be replaced with. It doesn't have to be existing token, but it won't autotranslate if not.").Value;
                return _config.Bind(category, "Enable Replacement", true, $"If true, then missing logs for this will be have its name obscured.").Value;
            }*/
            bool assignMain(string category)
            {
                return _config.Bind(category, "Enable Replacement", true, $"If true, then missing logs for this will be have its name obscured.").Value;
            }

            string assignReplacementToken(string category, string type, string newToken)
            {
                return _config.Bind<string>(category, $"{type} Replacement Token", newToken, "The new token that the original value will be replaced with").Value;
            }

            //god i fucking hate config files when its like this
            //twenty different variations
            //We're doing this instead of nesting the options INSIDE of the type check because
            //If they run it without it, and want to enable and see the rest of the settings, they cant until second launch
            var ifBody = assignMain("CharacterBodies");
            var ifBodyThenDefault = assignReplacementToken("CharacterBodies", "Default", "LOGBOOK_CATEGORY_MONSTER");
            var ifBodyThenSubtitleBool = _config.Bind<bool>("CharacterBodies", "Boss/Survivors Replacement Token Uses Subtitle", true, "If true, then if its a boss or survivor then it will use their subtitle token. If false, then it'll use the replacement token.").Value;
            var ifBodyThenSubtitleToken = _config.Bind<string>("CharacterBodies", $"Boss/Survivors Replacement Token", "???", "The new token that the original value will be replaced with. Keep empty to disable.").Value;

            /*"ENGI_SKIN_ALT1_NAME" : "EOD Tech",
             * "MAGE_SKIN_ALT1_NAME" : "Chrome",
             * "TOOLBOT_BODY_SUBTITLE" : "Right Tool for the Wrong Job",
             * "LOADER_BODY_SUBTITLE" : "Bionic Powerhouse",
             * =="ROBOBALLBOSS_BODY_SUBTITLE" : "Corrupted AI",
             */
            var ifBodyThenRobotToken = assignReplacementToken("CharacterBodies", "Mechanical", "ROBOBALLBOSS_BODY_SUBTITLE");

            if (ifBody)
                foreach (var bodyPrefab in BodyCatalog.allBodyPrefabs)
                {
                    var characterBody = bodyPrefab.GetComponent<CharacterBody>();
                    if (characterBody != null)
                    {
                        if (IsValid(characterBody.baseNameToken))
                        {
                            var deathRewards = bodyPrefab.GetComponent<DeathRewards>();
                            if (deathRewards && deathRewards.logUnlockableDef)
                            {
                                nameToken_to_unlockableDef.Add(characterBody.baseNameToken, deathRewards.logUnlockableDef);
                                var newToken = ifBodyThenDefault;

                                if (!characterBody.subtitleNameToken.IsNullOrWhiteSpace())
                                {
                                    //It's a boss
                                    //players have subtitles but dont have death rewards so they wont shop up
                                    //remember that
                                    //I'll put survivorcatalog above this
                                    //Loose fit
                                    if (ifBodyThenSubtitleBool)
                                        newToken = characterBody.subtitleNameToken;
                                    else
                                        //if no auto replace then use normal, but if normal is empty then use default token
                                        if (!ifBodyThenSubtitleToken.IsNullOrWhiteSpace())
                                            newToken = ifBodyThenSubtitleToken;
                                }
                                else
                                {
                                    if (characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)
                                        && !ifBodyThenRobotToken.IsNullOrWhiteSpace())
                                    {
                                        newToken = ifBodyThenRobotToken;
                                    }
                                    /*else
                                    {
                                        if (characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Void))
                                        {
                                            //only applies to voidtouched enemies so its not gonna show up in catalog.....
                                            //"VOIDSURVIVOR_PASSIVE_NAME" : "『V??oid Co??rruption】",
                                            //newToken = "VOIDSURVIVOR_PASSIVE_NAME";
                                        }
                                    }*/
                                }
                                replacementTokenDict.Add(characterBody.baseNameToken, newToken);
                            }
                        }
                    }
                }

            var ifItems = assignMain("Items");
            var ifItemsThenDefault = assignReplacementToken("Items", "Default", "LOGBOOK_CATEGORY_ITEM");

            if (ifItems)
                foreach (var itemDef in ItemCatalog.allItemDefs)
                {
                    if (IsValid(itemDef.nameToken)
                        && itemDef.unlockableDef)
                    {
                        nameToken_to_unlockableDef.Add(itemDef.nameToken, itemDef.unlockableDef);
                        replacementTokenDict.Add(itemDef.nameToken, ifItemsThenDefault);
                    }
                }

            var ifEquipment = assignMain("Equipment");
            var ifEquipmentThenDefault = assignReplacementToken("Equipment", "Default", "LOGBOOK_CATEGORY_EQUIPMENT");

            if (ifEquipment)
                foreach (var equipmentDef in EquipmentCatalog.equipmentDefs)
                {
                    if (IsValid(equipmentDef.nameToken)
                        && equipmentDef.unlockableDef)
                    {
                        nameToken_to_unlockableDef.Add(equipmentDef.nameToken, equipmentDef.unlockableDef);
                        replacementTokenDict.Add(equipmentDef.nameToken, ifEquipmentThenDefault);
                    }
                }

            var ifScene = assignMain("Scenes");
            var ifSceneThenDefault = assignReplacementToken("Scenes", "Default", "LOGBOOK_CATEGORY_STAGE");

            if (ifScene)
                foreach (var sceneDef in SceneCatalog.allSceneDefs)
                {
                    if (IsValid(sceneDef.nameToken))
                    {
                        var sceneUnlockableDef = UnlockableCatalog.GetUnlockableDef($"Logs.Stages.{sceneDef.cachedName}");
                        if (sceneUnlockableDef)
                        {
                            nameToken_to_unlockableDef.Add(sceneDef.nameToken, sceneUnlockableDef);
                            replacementTokenDict.Add(sceneDef.nameToken, ifSceneThenDefault);
                        }
                    }
                }

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in nameToken_to_unlockableDef)
            {
                
                sb.AppendLine($"{kvp.Key} => {kvp.Value.nameToken} ({kvp.Value.cachedName})");
            }
            _logger.LogMessage(sb);
        }

        public void Start()
        {
            On.RoR2.Language.GetLocalizedStringByToken += UIL_ModifyToken;
        }

        private string UIL_ModifyToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, RoR2.Language self, string token)
        {
            if (nameToken_to_unlockableDef.TryGetValue(token, out UnlockableDef unlockableDef))
            {
                var localUser = LocalUserManager.GetFirstLocalUser();
                if (localUser != null && localUser.userProfile != null)
                {
                    if (!localUser.userProfile.HasUnlockable(unlockableDef))
                    {
                        token = replacementTokenDict[token];
                    }
                }
            }
            return orig(self,token);
        }
    }
}