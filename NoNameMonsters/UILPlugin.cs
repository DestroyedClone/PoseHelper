using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RoR2;
using RoR2.Stats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using System.Text;


[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace UnidentifiedLandingz
{
    [BepInPlugin("com.DestroyedClone.UnidentifiedLanding", "Unidentified Landing", "1.0.0")]
    public class UnidentifiedLandingPlugin : BaseUnityPlugin
    {
        /* How does this mod work, and why is the code garbage?
         * First, we populate a dictionary of the tokens that need to be replaced based on the user's profile.
         * Then ingame, we check using the On.lang hook.
         * In order for it to update properly, we re-run the code with an "update" parameter of sorts.
         * 
         * 
         */
        public static ConfigFile _config;
        public static BepInEx.Logging.ManualLogSource _logger;

        public static readonly Dictionary<string, string> replacementTokenDict = new Dictionary<string, string>();

        public void Awake()
        {
            _config = Config;
            _logger = Logger;

            On.RoR2.UserProfile.OnLogin += UserProfile_OnLogin;
            RoR2.Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            On.RoR2.Language.GetLocalizedStringByToken += OverrideToken;
        }

        private string OverrideToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            if (replacementTokenDict.TryGetValue(token, out string replacementToken))
            {
                token = replacementToken;
            }
            return orig(self, token);
        }

        private void Stage_onStageStartGlobal(Stage stage)
        {
            var userProfile = LocalUserManager.GetFirstLocalUser()?.userProfile;
            if (userProfile != null)
            {
                UpdateAssignments(userProfile);
            }
        }

        private void UserProfile_OnLogin(On.RoR2.UserProfile.orig_OnLogin orig, UserProfile self)
        {
            orig(self);
            replacementTokenDict.Clear();
            UpdateAssignments(self);
        }

        private void UpdateAssignments(UserProfile userProfile)
        {
            var statSheet = userProfile.statSheet;
            if (statSheet != null)
            {
                AssignItem(statSheet);
                AssignEquipment(statSheet);
                AssignPickup(userProfile); //just lunar coin, also incomplete
                //AssignInteractable(userProfile); //incomplete
                //AssignMonster(userProfile);
                //AssignSurvivor(statSheet);
            }
        }

        private bool TokenIsValid(string token)
        {
            return !token.IsNullOrWhiteSpace() && !replacementTokenDict.ContainsKey(token);
        }

        bool AssignMain(string category)
        {
            return _config.Bind(category, "Enable Replacement", true, $"If true, then missing logs for this will be have its name obscured.").Value;
        }
        string AssignReplacementToken(string category, string type, string newToken, bool disable = false)
        {
            return _config.Bind<string>(category, $"{type} Replacement Token", newToken, $"The new token that the original value will be replaced with." +
                $" {(disable ? "Leave empty to disable." : "")}").Value;
        }

        private void AssignItem(StatSheet statSheet)
        {
            var addItem = AssignMain("Items");
            string itemReplacementToken = AssignReplacementToken("Items", "Default", "LOGBOOK_CATEGORY_ITEM");

            if (!addItem) return;
            foreach (var itemDef in ItemCatalog.allItemDefs)
            {
                if (itemDef.nameToken.IsNullOrWhiteSpace()) continue;

                //Better check than checking for unlockDef since some shit does not have it
                ulong itemTotalCount = statSheet.GetStatValueULong(PerItemStatDef.totalCollected.FindStatDef(itemDef.itemIndex));

                if (itemTotalCount <= 0) //aka has never had it
                {
                    replacementTokenDict[itemDef.nameToken] = itemReplacementToken;
                } else
                {
                    replacementTokenDict.Remove(itemDef.nameToken);
                    //this combo will be reached when 
                    //an unfound item is found during a playthrough, thus becoming found, and we should
                    //stop seeing the replacement token
                }
            }
        }

        private void AssignEquipment(StatSheet statSheet)
        {
            var addEquipment = AssignMain("Equipment");
            string equipmentReplacementToken = AssignReplacementToken("Equipment", "Default", "LOGBOOK_CATEGORY_EQUIPMENT");

            if (!addEquipment) return;
            foreach (var equipmentDef in EquipmentCatalog.equipmentDefs)
            {
                if (equipmentDef.nameToken.IsNullOrWhiteSpace()) continue;

                ulong equipmentHeldTime = statSheet.GetStatValueULong(PerEquipmentStatDef.totalTimeHeld.FindStatDef(equipmentDef.equipmentIndex));
                if (equipmentHeldTime <= 0)
                {
                    replacementTokenDict[equipmentDef.nameToken] = equipmentReplacementToken;
                } else
                {
                    replacementTokenDict.Remove(equipmentDef.nameToken);
                }
            }
        }

        private void AssignPickup(UserProfile userProfile)
        {
            var addPickup = _config.Bind("Pickups", "Enable Replacement", true, $"If true, then missing one will have its name obscured.").Value;
            string lunarCoinReplacementToken = AssignReplacementToken("Pickups", "Lunar Coin", "DUMMYINTERACTION_NAME", true);
            //"DUMMYINTERACTION_CONTEXT" is the same
            string lunarCoinNameToken = "PICKUP_LUNAR_COIN";
            string lunarCoinPickupToken = "LUNAR_COIN_PICKUP_CONTEXT";
            //technically void coins are here too but eh
            //idk what else is here

            if (!addPickup) return;

            if (!lunarCoinReplacementToken.IsNullOrWhiteSpace())
            {
                if (userProfile.totalCollectedCoins < 0)
                {
                    replacementTokenDict[lunarCoinNameToken] = lunarCoinReplacementToken;
                    replacementTokenDict[lunarCoinPickupToken] = lunarCoinReplacementToken;
                }
                else
                {
                    replacementTokenDict.Remove(lunarCoinNameToken);
                    replacementTokenDict.Remove(lunarCoinPickupToken);
                }
            }
        }

        private void AssignInteractable(UserProfile userProfile)
        {
            string AssignConfigEntry(string type, string newToken, string assocAchievementToken)
            {
                return _config.Bind<string>("Interactable", $"{type} Replacement Token", newToken, $"The new token that the original value will be replaced with, depending on the availability of a particular achievement.." +
                    $" Leave empty to disable." +
                    //for some reason this line is really fucking funny, it's like 5 lines in one
                    $"\nAssociated Achievement: {Language.GetString($"ACHIEVEMENT_{assocAchievementToken.ToUpper()}_NAME", "en")} - {Language.GetString($"ACHIEVEMENT_{assocAchievementToken.ToUpper()}_DESCRIPTION", "en")}").Value;
            }
            bool isValid(string text) { return !text.IsNullOrWhiteSpace(); }

            void AddReplacement(string originalToken, string newToken, string achievementName)
            {
                if (isValid(newToken))
                {
                    if (userProfile.HasAchievement(achievementName))
                    {
                        replacementTokenDict[originalToken] = newToken;
                    }
                    else
                    {
                        replacementTokenDict.Remove(originalToken);
                    }
                }
            }

            var addInteractable = _config.Bind("Interactable", "Enable Replacement", true, $"If true, then having no interactions with one will have its name obscured.").Value;

            //"Is This Bugged?"	Fail the Shrine of Chance 3 times in a row.
            string chanceShrineAchievement = "FAILSHRINECHANCE";
            string chanceShrineToken = "SHRINE_CHANCE_NAME";
            string chanceShrine = AssignConfigEntry("Chance Shrine", "", chanceShrineAchievement);

            //...Maybe One More.	Duplicate the same item 7 times in a row with a 3D Printer.
            string duplicatorAchievement = "REPEATEDLYDUPLICATEITEMS";
            string duplicatorToken = "DUPLICATOR_NAME";
            string duplicator = AssignConfigEntry("Duplicators", "", duplicatorAchievement);

            //Advancement	Complete a Teleporter event.
            string teleporterAchievement = "COMPLETETELEPORTER";
            string teleporterToken = "TELEPORTER_NAME";
            string teleporter = AssignConfigEntry("Teleporters", "", teleporterAchievement);

            //Automation Activation	Activate 6 turrets in a single run.
            //==Mechanic	Repair 30 drones or turrets.
            string turretsAchievement = "";
            string turretsToken = "TURRET1_INTERACTABLE_NAME";
            string turrets = AssignConfigEntry("Turrets", "", turretsAchievement);
            
            //Newtist	Discover and activate 8 unique Newt Altars.
            string newtAltarAchievement = "";
            string newtAltarToken = "NEWT_STATUE_NAME";
            string newtAltar = AssignConfigEntry("Newt Altars", "", newtAltarAchievement);
            
            //Prismatically Aligned	Complete a Prismatic Trial.
            string prismaticTrialAchievement = "";
            string prismaticTrialToken = "";
            string prismaticTrial = AssignConfigEntry("Prismatic Trial Stuff", "", prismaticTrialAchievement);
            
            //Warmonger	Complete 3 Combat Shrines in a single stage.
            string combatShrineAchievement = "";
            string combatShrineToken = "";
            string combatShrine = AssignConfigEntry("Combat Shrine", "", combatShrineAchievement);
            
            //Ascendant	Defeat the Teleporter bosses after activating 2 Shrines of the Mountain.
            string mountainShrineAchievement = "";
            string mountainShrineToken = "";
            string mountainShrine = AssignConfigEntry("Mountain Shrine", "", mountainShrineAchievement);
            
            //Blackout	Defeat the unique guardian of Gilded Coast without any beacons deactivating.
            string goldBeaconAchievement = "";
            string goldBeaconToken = "";
            string goldBeacon = AssignConfigEntry("", "", goldBeaconAchievement);
            
            //Cleanup Duty	Destroy 20 flying rocks in Sky Meadow.
            string flyingRocksAchievement = "";
            string flyingRocksToken = "";
            string flyingRocks = AssignConfigEntry("", "", flyingRocksAchievement);
            
            //Cosmic Explorer	Discover and enter three unique portals.
            string portalAchievement = "";
            string portalToken = "";
            string portal = AssignConfigEntry("", "", portalAchievement);
            
            //One with the Woods	Fully upgrade a Shrine of the Woods.
            string woodsShrineAchievement = "";
            string woodsShrineToken = "";
            string woodsShrine = AssignConfigEntry("", "", woodsShrineAchievement);
            
            //[REDACTED]	Open the Timed Security Chest on Rallypoint Delta.
            string timedChestAchievement = "";
            string timedChestToken = "";
            string timedChest = AssignConfigEntry("", "", timedChestAchievement);
            
            //Captain: Worth Every Penny	As Captain, repair and recruit a TC-280 Prototype.
            string tc280DroneAchievement = "";
            string tc280DroneToken = "";
            string tc280Drone = AssignConfigEntry("TC-280 Prototype", "", tc280DroneAchievement);
            //
            //"STEAM_LOBBY_INVISIBLE": "Invisible",
            //		"UNIDENTIFIED" : "???",
            //"UNIDENTIFIED_DESCRIPTION" : "You have not met the prerequisites for this Challenge yet.",
            //"UNIDENTIFIED_KILLER_NAME" : "The Planet",
            //"ARTIFACT_TRIAL_CONTROLLER_NAME" : "Unknown Artifact",
            //"DRONE_MEGA_CONTEXT": "Repair ???",
            //"LOCKEDTREEBOT_NAME" : "Broken Robot",
            //"LOCKEDTREEBOT_CONTEXT" : "Repair",

            if (!addInteractable) return;
            AddReplacement(chanceShrineToken, chanceShrine, chanceShrineAchievement);
            AddReplacement(duplicator, duplicatorToken, duplicatorAchievement);
            AddReplacement(teleporter, teleporterToken, teleporterAchievement);
            AddReplacement(turrets, turretsToken, turretsAchievement);
            AddReplacement(newtAltar, newtAltarToken, newtAltarAchievement);
            AddReplacement(prismaticTrial, prismaticTrialToken, prismaticTrialAchievement);
            AddReplacement(combatShrine, combatShrineToken, combatShrineAchievement);
            AddReplacement(mountainShrine, mountainShrineToken, mountainShrineAchievement);
            AddReplacement(goldBeacon, goldBeaconToken, goldBeaconAchievement);
            AddReplacement(flyingRocks, flyingRocksToken, flyingRocksAchievement);
            AddReplacement(portal, portalToken, portalAchievement);
            AddReplacement(woodsShrine, woodsShrineToken, woodsShrineAchievement);
            AddReplacement(timedChest, timedChestToken, timedChestAchievement);
            AddReplacement(tc280Drone, tc280DroneToken, tc280DroneAchievement);


        }

        private void AssignMonster(UserProfile userProfile)
        {
            var addMonster = AssignMain("Monster");
            string monsterToken = AssignReplacementToken("Monster", "Default", "LOGBOOK_CATEGORY_MONSTER", true);
            bool useSubtitleIfAvailable = _config.Bind("Monster", "Use Subtitle if Available", true, "If true, then if the character has a subtitle, then it will use that subtitle.").Value;
            string robotToken = AssignReplacementToken("Monster", "Mechanical", "ROBOBALLBOSS_BODY_SUBTITLE", true);

            if (!addMonster) return;
            #region hardcodes
            bool gupChecked = false; //for gup and gip and gup and gout and grub and grout and snout and sinp andsnifesfnsjkfds
            bool twistedScavChecked = false;


            foreach (var bodyPrefab in BodyCatalog.allBodyPrefabs)
            {
                var characterBody = bodyPrefab.GetComponent<CharacterBody>();
                if (!characterBody || characterBody.baseNameToken.IsNullOrWhiteSpace()) continue;

                var deathRewards = bodyPrefab.GetComponent<DeathRewards>();
                if (deathRewards != null)
                {
                    var unlockableDef = deathRewards.logUnlockableDef;

                }
                var replacementToken = monsterToken;

                if (useSubtitleIfAvailable
                    && !characterBody.subtitleNameToken.IsNullOrWhiteSpace())
                {
                    replacementToken = characterBody.subtitleNameToken;
                }
                else
                {
                    if (characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                        replacementToken = robotToken;
                }
                replacementTokenDict[characterBody.baseNameToken] = replacementToken;
            }
            #endregion

        }

        private void AssignSurvivor(StatSheet statSheet)
        {
            var addSurvivor = AssignMain("Survivor");

            if (!addSurvivor) return;
        }
    }
}
