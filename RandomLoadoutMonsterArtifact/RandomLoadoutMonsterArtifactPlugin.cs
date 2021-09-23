using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Collections.Generic;
using EntityStates.AI;
using System.Linq;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace RandomLoadoutMonsterArtifact
{
    [BepInPlugin("com.DestroyedClone.RandomLoadoutArtifact", "Random Loadout Artifact", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(ArtifactAPI))]
    public class RandomLoadoutMonsterArtifactPlugin : BaseUnityPlugin
    {
        public static ArtifactDef RandomLoadoutAll = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ArtifactDef RandomLoadoutMonster = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ArtifactDef RandomLoadoutMonsterSurvivors = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ArtifactDef EvolRef = Resources.Load<ArtifactDef>("artifactdefs/MonsterTeamGainsItems");

        public static BodyIndex[] survivorBodyIndices = new BodyIndex[] { };

        public void Awake()
        {
            InitializeArtifact();
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            List<BodyIndex> bodyIndices = new List<BodyIndex>();
            foreach (var survivor in SurvivorCatalog.allSurvivorDefs)
            {
                if (survivor.bodyPrefab.GetComponent<CharacterBody>())
                {
                    bodyIndices.Add(survivor.bodyPrefab.GetComponent<CharacterBody>().bodyIndex);
                }
                survivorBodyIndices = bodyIndices.ToArray();
            }


            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (!obj.master) return;
            bool randomize = false;
            if (RunArtifactManager.instance.IsArtifactEnabled(RandomLoadoutAll))
            {
                randomize = true;
            }
            else
            {
                if (obj.teamComponent && obj.teamComponent.teamIndex != TeamIndex.Player)
                {
                    if (RunArtifactManager.instance.IsArtifactEnabled(RandomLoadoutMonster))
                    {
                        randomize = true;
                    } else if (RunArtifactManager.instance.IsArtifactEnabled(RandomLoadoutMonsterSurvivors) && survivorBodyIndices.Contains(obj.bodyIndex))
                    {
                        randomize = true;
                    }
                }
            }
            if (randomize) ApplyRandomLoadout(obj);
            
            
        }

        private static void ApplyRandomLoadout(CharacterBody characterBody)
        {
            BodyIndex bodyIndex = characterBody.bodyIndex;
            Loadout loadout = new Loadout();

            var skillSlotCount = Loadout.BodyLoadoutManager.GetSkillSlotCountForBody(bodyIndex);
            for (int skillSlotIndex = 0; skillSlotIndex < skillSlotCount; skillSlotIndex++)
            {
                var skillVariantCount = GetSkillVariantCount(bodyIndex, skillSlotIndex);
                var skillVariantIndex = Random.Range(0, skillVariantCount);
                loadout.bodyLoadoutManager.SetSkillVariant(bodyIndex, skillSlotIndex, (uint)skillVariantIndex);
            }

            if (characterBody.master)
            {
                characterBody.master.SetLoadoutServer(loadout);
            }
            if (characterBody)
            {
                characterBody.SetLoadoutServer(loadout);
            }
        }

        private static int GetSkillVariantCount(BodyIndex bodyIndex, int skillSlot)
        {
            return Loadout.BodyLoadoutManager.allBodyInfos[(int)bodyIndex].prefabSkillSlots[skillSlot].skillFamily.variants.Length;
        }


        public static void InitializeArtifact()
        {
            RandomLoadoutAll.nameToken = "Artifact of Tyfortosi (All)";
            RandomLoadoutAll.descriptionToken = "Randomizes loadouts for survivors and monsters alike.";
            RandomLoadoutAll.smallIconDeselectedSprite = EvolRef.smallIconDeselectedSprite;
            RandomLoadoutAll.smallIconSelectedSprite = EvolRef.smallIconSelectedSprite;
            ArtifactAPI.Add(RandomLoadoutAll);


            RandomLoadoutMonster.nameToken = "Artifact of Tyfortosi (Monster)";
            RandomLoadoutMonster.descriptionToken = "Randomizes loadouts for all monsters. Has minimal effect.";
            RandomLoadoutMonster.smallIconDeselectedSprite = EvolRef.smallIconDeselectedSprite;
            RandomLoadoutMonster.smallIconSelectedSprite = EvolRef.smallIconSelectedSprite;
            ArtifactAPI.Add(RandomLoadoutMonster);

            RandomLoadoutMonsterSurvivors.nameToken = "Artifact of Tyfortosi (Enemy Survivor)";
            RandomLoadoutMonsterSurvivors.descriptionToken = "Randomizes loadouts for survivors that spawn as monsters.";
            RandomLoadoutMonsterSurvivors.smallIconDeselectedSprite = EvolRef.smallIconDeselectedSprite;
            RandomLoadoutMonsterSurvivors.smallIconSelectedSprite = EvolRef.smallIconSelectedSprite;
            ArtifactAPI.Add(RandomLoadoutMonsterSurvivors);
        }
    }
}
