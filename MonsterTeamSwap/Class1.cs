using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace MonsterTeamSwap
{
    [BepInPlugin("com.DestroyedClone.MonsterTeamSwap", "Monster Team Swap", "1.0.2")]
    public class Class1 : BaseUnityPlugin
    {
        public static ConfigEntry<string> cfgNone;
        public static ConfigEntry<string> cfgNeutral;
        public static ConfigEntry<string> cfgPlayer;
        public static ConfigEntry<string> cfgMonster;
        public static ConfigEntry<string> cfgLunar;
        public static ConfigEntry<string> cfgVoid;
        public static ConfigEntry<bool> cfgArenaKill;
        //public static ConfigEntry<bool> cfgChildTeamIsParent;
        public static ConfigEntry<bool> cfgVoidStaysVoid;

        public static List<CharacterMaster> arenaModifiedCharacterMasters = new List<CharacterMaster>();

        //public static ConfigEntry<string> cfgOutput;

        public static Dictionary<BodyIndex, TeamIndex> bodyIndex_to_teamIndex = new Dictionary<BodyIndex, TeamIndex>();

        internal static BepInEx.Logging.ManualLogSource _logger;

        internal static bool shouldStartTrackingArenaSpawns = false;

        public void Awake()
        {
            _logger = Logger;
            SetupConfig();

            // ImpBody:Lunar
            On.RoR2.DirectorCore.TrySpawnObject += OverrideTeamSpawn;

            if (cfgArenaKill.Value)
            {
                On.RoR2.ArenaMissionController.OnEnable += StartTracking;
                On.RoR2.ArenaMissionController.OnDisable += StopTracking;
            }
        }

        private void StartTracking(On.RoR2.ArenaMissionController.orig_OnEnable orig, ArenaMissionController self)
        {
            orig(self);
            shouldStartTrackingArenaSpawns = true;
            On.RoR2.ArenaMissionController.EndRound += Arena_KillTracked;
        }

        private void StopTracking(On.RoR2.ArenaMissionController.orig_OnDisable orig, ArenaMissionController self)
        {
            orig(self);
            shouldStartTrackingArenaSpawns = false;
            arenaModifiedCharacterMasters.Clear();
            On.RoR2.ArenaMissionController.EndRound -= Arena_KillTracked;
        }

        private void Arena_KillTracked(On.RoR2.ArenaMissionController.orig_EndRound orig, ArenaMissionController self)
        {
            orig(self);
            int amt = 0;
            foreach (var enemy in arenaModifiedCharacterMasters.ToArray())
            {
                if (!enemy) continue;
                enemy.GetBody()?.healthComponent.Suicide(self.gameObject, self.gameObject, DamageType.VoidDeath);
                amt++;
            }
            arenaModifiedCharacterMasters.Clear();
        }

        public void SetupConfig()
        {
            string catName = "Team Index Overrides";
            string catName2 = "Exceptions";
            string description = "Add the names of Bodies you want to force switch the teams of.";
            cfgNone = Config.Bind(catName, "None", "", description + " This is the name of a Team Index, not an exclusion list.");
            cfgNeutral = Config.Bind(catName, "Neutral", "ImpBody,ImpBossBody", description);
            cfgPlayer = Config.Bind(catName, "Player", "", description + " Careful, without friendly fire you won't be able to damage them.");
            cfgMonster = Config.Bind(catName, "Monster", "", description);
            cfgLunar = Config.Bind(catName, "Lunar", "LunarGolemBody,LunarWispBody,LunarExploderBody", description);
            cfgVoid = Config.Bind(catName, "Void", "NullifierBody,VoidJailerBody,VoidDevastatorBody,VoidBarnacleBody", description);
            cfgVoidStaysVoid = Config.Bind(catName2, "Void Stays Void", true, "If something is already Void, such as those spawned by void camps, should it stay on the Void team?");
            cfgArenaKill = Config.Bind(catName2, "Kill Previously Monster On Void Fields Cell Clear", true, "If true, then anything spawned in the Void Fields that used to be on the Monster team will be killed when the cell kills off all the monsters.");
            //cfgChildTeamIsParent = Config.Bind(catName2, "Summoned Body Belongs To Masters Team", true, "If true, then the summoned body will stay belonging to the team of the master that summoned it.");
        }

        private GameObject OverrideTeamSpawn(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            if (directorSpawnRequest.spawnCard && directorSpawnRequest.spawnCard.prefab)
            {
                var characterMaster = directorSpawnRequest.spawnCard.prefab.GetComponent<CharacterMaster>();
                if (characterMaster)
                {
                    if (characterMaster.teamIndex == TeamIndex.Void && cfgVoidStaysVoid.Value)
                    {
                        //Chat.AddMessage($"Keeping teamIndex of {characterMaster.gameObject.name} as Void due to pre-infestation.");
                    } else
                    {
                        if (characterMaster.bodyPrefab)
                        {
                            var bodyIndex = characterMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                            if (bodyIndex_to_teamIndex.TryGetValue(bodyIndex, out TeamIndex teamIndex))
                            {
                                /*if (cfgChildTeamIsParent.Value 
                                    && characterMaster.)
                                {
                                    goto Label_Resume;
                                }*/

                                if (shouldStartTrackingArenaSpawns)
                                {
                                    if (characterMaster.teamIndex == TeamIndex.Monster)
                                    {
                                        directorSpawnRequest.onSpawnedServer += (result) =>
                                        {
                                            SpawnCard.SpawnResult spawnResult = result;
                                            if (spawnResult.success)
                                            {
                                                arenaModifiedCharacterMasters.Add(characterMaster);
                                            }
                                        };
                                    }
                                }
                                //Chat.AddMessage($"Overriding teamIndex of {characterMaster.gameObject.name} to {teamIndex}");
                                directorSpawnRequest.teamIndexOverride = teamIndex;
                            }
                        }
                    }
                }
            }
            Label_Resume:
            var original = orig(self, directorSpawnRequest);
            return original;
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.BodyCatalog))]
        public static void SetupDictionary()
        {
            _logger.LogMessage("Setting up!");

            //Delimiter credit: https://github.com/KomradeSpectre/AetheriumMod/blob/c6fe6e8a30c3faf5087802ad7e5d88020748a766/Aetherium/Items/AccursedPotion.cs#L349
            TeamIndex teamIndex = TeamIndex.None;
            foreach (var value in new string[] 
                { cfgNone.Value, cfgNeutral.Value, cfgPlayer.Value, cfgMonster.Value, cfgLunar.Value, cfgVoid.Value })
            {
                _logger.LogMessage($"Adding bodies to TeamIndex: {teamIndex}");
                var valueArray = value.Split(',');
                if (valueArray.Length > 0)
                {
                    foreach (string valueToTest in valueArray)
                    {
                        var bodyIndex = BodyCatalog.FindBodyIndex(valueToTest);
                        if (bodyIndex == BodyIndex.None)
                        {
                            continue;
                        }
                        bodyIndex_to_teamIndex.Add(bodyIndex, teamIndex);
                        _logger.LogMessage(valueToTest);
                    }
                }
                teamIndex++;
            }

            //bodyIndex_to_teamIndex.Add(BodyCatalog.FindBodyIndex("ImpBody"), TeamIndex.Lunar);
            //bodyIndex_to_teamIndex.Add(BodyCatalog.FindBodyIndex("ImpBossBody"), TeamIndex.Lunar);
        }
    }
}