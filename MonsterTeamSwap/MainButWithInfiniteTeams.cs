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
    [BepInPlugin("com.DestroyedClone.MonsterTeamSwap", "Monster Team Swap", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public static ConfigEntry<string> cfgNone;
        public static ConfigEntry<string> cfgNeutral;
        public static ConfigEntry<string> cfgPlayer;
        public static ConfigEntry<string> cfgMonster;
        public static ConfigEntry<string> cfgLunar;
        public static ConfigEntry<string> cfgVoid;

        public static Dictionary<GameObject, TeamDef> kvp = new Dictionary<GameObject, TeamDef>();

        //public static ConfigEntry<string> cfgOutput;

        public static Dictionary<BodyIndex, TeamIndex> bodyIndex_to_teamIndex = new Dictionary<BodyIndex, TeamIndex>();

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            SetupConfig();

            // ImpBody:Lunar
            On.RoR2.DirectorCore.TrySpawnObject += OverrideTeamSpawn;

            //CreatingList();
            RoR2Application.onLoad += () =>
            {
                CreateTeamDef("TEAM_VOID_NAME");
                CreateTeamDef("TEAM_A_NAME");
                CreateTeamDef("TEAM_B_NAME");
                CreateTeamDef("TEAM_C_NAME");
                for (int i = 0; i < TeamCatalog.teamDefs.Length; i++)
                {
                    TeamDef teamDef = TeamCatalog.teamDefs[i];
                    Debug.Log($"[{i}] {teamDef.nameToken}");
                }
            };
        }


        public TeamDef CreateTeamDef(string nameToken)
        {
            TeamDef teamDef = new TeamDef
            {
                nameToken = "TEAM_VOID_NAME",
                softCharacterLimit = 40,
                friendlyFireScaling = 2f,
                levelUpEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/LevelUpEffectEnemy"),
                levelUpSound = "Play_UI_levelUp_enemy"
            };
            TeamCatalog.Register((TeamIndex)TeamCatalog.teamDefs.Length-1, teamDef);
            return teamDef;
        }

        public void CreatingList()
        {
            kvp.Add(gameObject, new TeamDef());
        }

        public void SetupConfig()
        {

        }

        private GameObject OverrideTeamSpawn(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            if (directorSpawnRequest.spawnCard && directorSpawnRequest.spawnCard.prefab)
            {
                var characterMaster = directorSpawnRequest.spawnCard.prefab.GetComponent<CharacterMaster>();
                if (characterMaster)
                {
                    var bodyIndex = characterMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                    if (bodyIndex_to_teamIndex.TryGetValue(bodyIndex, out TeamIndex teamIndex))
                    {
                        //_logger.LogMessage($"Overriding teamIndex to {teamIndex}");
                        directorSpawnRequest.teamIndexOverride = teamIndex;
                    }
                }
            }
            var original = orig(self, directorSpawnRequest);
            return original;
        }

        //[RoR2.SystemInitializer(dependencies: typeof(RoR2.BodyCatalog))]
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