using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.CompilerServices;
using static RoR2.RoR2Content.Items;
using System.Linq;
using RoR2.Skills;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace EnemySurvivors
{
    [BepInPlugin("com.DestroyedClone.EnemySurvivors", "EnemySurvivors", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2API.Utils.R2APISubmoduleDependency(new string[] {
        nameof(DirectorAPI),
        nameof(ItemAPI),
        nameof(RecalculateStatsAPI)
    })]
    public class SurvivorsMod : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource _logger;

        public static ConfigEntry<bool> cfgLoop;
        public static ConfigEntry<float> cfgCommonMult;
        public static ConfigEntry<float> cfgUncommonMult;
        public static ConfigEntry<float> cfgLegendaryMult;

        //public static ConfigEntry<bool> cfgHereticEnable;
        public static ConfigEntry<float> cfgHeresyScale;


        public static DirectorCard[] survivorDirectorCards;
        public static CharacterSpawnCard[] survivorSpawnCards;
        public static DirectorAPI.DirectorCardHolder[] directorCardHolders;
        public static BodyIndex hereticBodyIndex;
        public static BodyIndex[] survivorBodyIndices;
        public static ItemDef minusOnePercentDamage;

        public void Awake() { CreateItems(); }

        public void Start()
        {
            _logger = Logger;
            cfgLoop = Config.Bind("Spawning", "Only Loop", false, "If true, then survivors will only spawn after looping.");
            cfgCommonMult = Config.Bind("Items", "Common", 2f, "The value, multiplied by the stage count, of the amount of white items given. Rounded.");
            cfgUncommonMult = Config.Bind("Items", "Uncommon", 0.4f, "The value, multiplied by the stage count, of the amount of green items given. Rounded.");
            cfgLegendaryMult = Config.Bind("Items", "Legendary", 0.3f, "The value, multiplied by the stage count, of the amount of red items given. Rounded.");
            //cfgHereticEnable = Config.Bind("Spawning", "Enable Heretic", true, "If true, then the Heretic will spawn as well");
            cfgHeresyScale = Config.Bind("Items", "Heretic + Heresy Set", 0.5f, "The value, multiplied by the stage count, of the amount of each stack of the heresy set to give to Heretic.");

            SetupDirectorCards();
            DirectorAPI.MonsterActions += AddSurvivors;
                On.RoR2.DirectorCore.TrySpawnObject += DirectorCore_TrySpawnObject;
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.MonsterTeamSwap"))
            {
            }
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory && sender.inventory.GetItemCount(minusOnePercentDamage) > 0)
            {
                args.damageMultAdd -= sender.inventory.GetItemCount(minusOnePercentDamage) * 0.01f;
            }
        }

        public void CreateItems()
        {

            minusOnePercentDamage = ScriptableObject.CreateInstance<ItemDef>();
            minusOnePercentDamage.name = "SAE_ITEM_REDUCE_DAMAGE_ONE_PERCENT";
            minusOnePercentDamage.canRemove = false;
            minusOnePercentDamage.descriptionToken = "";
            minusOnePercentDamage.hidden = false;
            minusOnePercentDamage.loreToken = "";
            minusOnePercentDamage.nameToken = "SAE_ITEM_REDUCE_DAMAGE_ONE_PERCENT";
            minusOnePercentDamage.pickupToken = "";
            minusOnePercentDamage.tags = new ItemTag[] { };
            minusOnePercentDamage.tier = ItemTier.NoTier;
            ItemAPI.Add(new CustomItem(minusOnePercentDamage, new ItemDisplayRule[] { }));
        }

        private GameObject DirectorCore_TrySpawnObject(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            GameObject original = orig(self, directorSpawnRequest);
            if (directorSpawnRequest.spawnCard && directorSpawnRequest.spawnCard.prefab)
            {
                var characterMaster = directorSpawnRequest.spawnCard.prefab.GetComponent<CharacterMaster>();
                if (characterMaster)
                {
                    var bodyIndex = characterMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;

                    Logger.LogMessage($"Object {directorSpawnRequest.spawnCard.prefab.name} is {(survivorBodyIndices.Contains(bodyIndex) ? "" : "NOT")} in the list.");

                    if (survivorBodyIndices.Contains(bodyIndex))
                    {
                        directorSpawnRequest.teamIndexOverride = TeamIndex.Neutral;

                        ApplyRandomLoadout(original.GetComponent<CharacterMaster>(), bodyIndex);
                    }
                }
            }
            return original;
        }
        private void ApplyRandomLoadout(CharacterMaster characterMaster, BodyIndex bodyIndex)
        {
            if (!characterMaster)
            {
                Logger.LogWarning("Attempted to modify loadout of null CharacterMaster!");
            }
            Loadout loadout = new Loadout();

            var skillSlotCount = Loadout.BodyLoadoutManager.GetSkillSlotCountForBody(bodyIndex);
            for (int skillSlotIndex = 0; skillSlotIndex < skillSlotCount; skillSlotIndex++)
            {
                var skillVariantCount = GetSkillVariantCount(bodyIndex, skillSlotIndex);
                var skillVariantIndex = Random.Range(0, skillVariantCount);
                loadout.bodyLoadoutManager.SetSkillVariant(bodyIndex, skillSlotIndex, (uint)skillVariantIndex);
            }

            if (characterMaster)
            {
                characterMaster.SetLoadoutServer(loadout);
            }
        }
        private static int GetSkillVariantCount(BodyIndex bodyIndex, int skillSlot)
        {
            return Loadout.BodyLoadoutManager.allBodyInfos[(int)bodyIndex].prefabSkillSlots[skillSlot].skillFamily.variants.Length;
        }

        private void AddSurvivors(List<DirectorAPI.DirectorCardHolder> list, DirectorAPI.StageInfo stage)
        {
            switch (stage.stage)
            {
                case DirectorAPI.Stage.VoidCell:
                case DirectorAPI.Stage.ArtifactReliquary:
                case DirectorAPI.Stage.Bazaar:
                case DirectorAPI.Stage.Commencement:
                case DirectorAPI.Stage.MomentFractured:
                case DirectorAPI.Stage.MomentWhole:
                    break;
                default:
                    foreach (var directorCardHolder in directorCardHolders)
                    {
                        if (!list.Contains(directorCardHolder))
                        {
                            list.Add(directorCardHolder);
                            //_logger.LogMessage("Added to directorapi " + directorCardHolder.Card.spawnCard.prefab.name);
                        }
                    }
                    break;
            }
        }

        [RoR2.SystemInitializer(dependencies: new System.Type[]
        {
            typeof(BodyCatalog),
            typeof(RoR2.SurvivorCatalog),
            typeof(RoR2.MasterCatalog),
        })]
        public static void SetupDirectorCards()
        {
            _logger.LogMessage("Setting up the Director Cards");
            hereticBodyIndex = BodyCatalog.FindBodyIndex("HereticBody");
            List<DirectorCard> sdc = new List<DirectorCard>();
            List<CharacterSpawnCard> spc = new List<CharacterSpawnCard>();
            List<DirectorAPI.DirectorCardHolder> dch = new List<DirectorAPI.DirectorCardHolder>();

            List<CharacterMaster> characterMasters = new List<CharacterMaster>();
            var bodyIndexList = new List<BodyIndex>();

            foreach (var survivorDef in SurvivorCatalog.allSurvivorDefs)
            {
                var prefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(BodyCatalog.FindBodyIndex(survivorDef.bodyPrefab))).GetComponent<CharacterMaster>();
                //_logger.LogMessage($"Adding Prefab {prefab.name}");
                characterMasters.Add(prefab);
            }

            foreach (var characterMaster in characterMasters)
            {
                var characterBody = characterMaster.bodyPrefab.GetComponent<CharacterBody>();
                bodyIndexList.Add(characterBody.bodyIndex);

                if (characterMaster)
                {
                    var itemGranter = characterMaster.gameObject.AddComponent<ItemSetterUpper>();
                    itemGranter.characterMaster = characterMaster ?? null;
                    itemGranter.inventory = characterMaster.inventory ?? null;
                    itemGranter.survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(itemGranter.characterMaster.bodyPrefab);
                }

                CharacterSpawnCard survivorSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
                survivorSpawnCard.directorCreditCost = 150;
                survivorSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
                //equipmentToGrant = ;
                survivorSpawnCard.forbiddenAsBoss = true;
                survivorSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.None;
                survivorSpawnCard.hullSize = characterBody.hullClassification;
                survivorSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
                survivorSpawnCard.noElites = true;
                survivorSpawnCard.occupyPosition = false;
                survivorSpawnCard.sendOverNetwork = true;
                survivorSpawnCard.prefab = characterMaster.gameObject;
                spc.Add(survivorSpawnCard);

                DirectorCard survivorDirectorCard = new DirectorCard
                {
                    spawnCard = survivorSpawnCard,
                    selectionWeight = 1,
                    allowAmbushSpawn = false, //aka Horde of Many
                    preventOverhead = false,
                    minimumStageCompletions = cfgLoop.Value ? 5 : 0,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
                };
                sdc.Add(survivorDirectorCard);
                DirectorAPI.DirectorCardHolder survivorDirectorCardHolder = new DirectorAPI.DirectorCardHolder
                {
                    Card = survivorDirectorCard,
                    MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                    InteractableCategory = DirectorAPI.InteractableCategory.None
                };
                dch.Add(survivorDirectorCardHolder);

            }
            survivorDirectorCards = sdc.ToArray();
            survivorSpawnCards = spc.ToArray();
            directorCardHolders = dch.ToArray();
            survivorBodyIndices = bodyIndexList.ToArray();
            //Debug.LogWarning("Length of Holders: "+directorCardHolders.Length);
        }

        private class ItemSetterUpper : MonoBehaviour, ILifeBehavior
        {
            public SurvivorDef survivorDef;
            public CharacterMaster characterMaster;
            public Inventory inventory;
            public bool replaceBaseStats = false;

            public void OnDeathStart()
            {
            }

            public void Awake()
            {
                if (!characterMaster) characterMaster = gameObject.GetComponent<CharacterMaster>();
                if (!inventory) inventory = characterMaster.inventory ?? gameObject.GetComponent<Inventory>();
                if (!survivorDef) survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(characterMaster.bodyPrefab);


                if (inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
                    return;
                if (characterMaster.teamIndex != TeamIndex.Neutral)
                {
                    characterMaster.teamIndex = TeamIndex.Neutral;
                }


                var body = gameObject.GetComponent<CharacterMaster>().GetBody();

                Chat.AddMessage($"{body.GetDisplayName()} has joined the game.");

                if (Run.instance && !gameObject.GetComponent<ScavengerItemGranter>())
                {
                    var run = Run.instance;
                    var com = gameObject.AddComponent<ScavengerItemGranter>();
                    com.overwriteEquipment = false;
                    com.tier1StackSize = Mathf.RoundToInt(run.stageClearCount * cfgCommonMult.Value);
                    com.tier2StackSize = Mathf.RoundToInt(run.stageClearCount * cfgUncommonMult.Value);
                    com.tier3StackSize = Mathf.RoundToInt(run.stageClearCount * cfgLegendaryMult.Value);
                }
                if (body.bodyIndex == hereticBodyIndex)
                {
                    GiveHeresyItemsIfMissing();
                }
                GiveItemsForStatModification(body);

                //Reduce
            }

            public void GiveHeresyItemsIfMissing()
            {
                int requiredCount = Mathf.RoundToInt(Run.instance.stageClearCount * cfgHeresyScale.Value);
                foreach (ItemDef itemDef in new ItemDef[] { LunarPrimaryReplacement, LunarSecondaryReplacement, LunarUtilityReplacement, LunarSpecialReplacement })
                {
                    var difference = requiredCount - inventory.GetItemCount(itemDef);
                    if (difference > 0)
                    {
                        inventory.GiveItem(itemDef, difference);
                    }
                }
            }

            public void GiveItemsForStatModification(CharacterBody characterBody)
            {
                int damageReductionPercentage = 20;
                int healthMultPercent = 0; //needs to be in 10% increments
                var skillLocator = characterBody.skillLocator;
                if (survivorDef == RoR2Content.Survivors.Bandit2)
                {
                    damageReductionPercentage = 30;
                }
                else if (survivorDef == RoR2Content.Survivors.Captain) //utility
                {
                    damageReductionPercentage = 30;
                    if (skillLocator.utility.skillDef != Resources.Load<RoR2.Skills.SkillDef>("skilldefs/captainbody/callairstrike"))
                    {
                        damageReductionPercentage = 40;
                    }
                }
                else if (survivorDef == RoR2Content.Survivors.Commando)
                {
                    damageReductionPercentage = 30;
                }
                else if (survivorDef == RoR2Content.Survivors.Croco)
                {
                    healthMultPercent = 10;
                }
                else if (survivorDef == RoR2Content.Survivors.Engi)
                {
                    damageReductionPercentage = 40;
                }
                else if (survivorDef == RoR2Content.Survivors.Huntress)
                {
                    damageReductionPercentage = 30;
                }
                else if (survivorDef == RoR2Content.Survivors.Loader)
                {
                    healthMultPercent = 20;
                }
                else if (survivorDef == RoR2Content.Survivors.Mage) //secondary
                {
                    damageReductionPercentage = 20;
                }
                else if (survivorDef == RoR2Content.Survivors.Merc)
                {
                    damageReductionPercentage = 20;
                    if (skillLocator.special.skillDef == Resources.Load<RoR2.Skills.SkillDef>("skilldefs/mercbody/mercbodyevis"))
                    {
                        damageReductionPercentage = 50;
                    }
                }
                else if (survivorDef == RoR2Content.Survivors.Toolbot)
                {
                    damageReductionPercentage = 20;
                    SkillDef grenadeLauncherDef = Resources.Load<SkillDef>("skilldefs/toolbotbody/toolbotbodyfiregrenadelauncher");
                    SkillDef rebarDef = Resources.Load<SkillDef>("skilldefs/toolbotbody/toolbotbodyfirespear");
                    if (skillLocator.primary.skillDef == rebarDef || skillLocator.secondary.skillDef == rebarDef)
                    {
                        damageReductionPercentage += 10;
                    }
                    if (skillLocator.special.skillDef == Resources.Load<SkillDef>("skilldefs/toolbotbody/toolbotdualwield"))
                    {
                        damageReductionPercentage += 10;
                    }
                }
                else if (survivorDef == RoR2Content.Survivors.Treebot)
                {
                }
                else if (characterBody.bodyIndex == hereticBodyIndex)
                {
                    damageReductionPercentage = 30;
                }

                inventory.GiveItem(minusOnePercentDamage, damageReductionPercentage);
                inventory.GiveItem(BoostHp, healthMultPercent / 10);
            }

            public void ModifyBaseStats(CharacterBody characterBody)
            {
                //value isn't constant so...
                float damageReduction = 0.2f;
                float healthMultiplier = 1f;
                var skillLocator = characterBody.skillLocator;
                if (survivorDef == RoR2Content.Survivors.Bandit2)
                {
                    damageReduction += 0.1f;
                }
                else if (survivorDef == RoR2Content.Survivors.Captain) //utility
                {
                    damageReduction += 0.2f;
                }
                else if (survivorDef == RoR2Content.Survivors.Commando)
                {

                }
                else if (survivorDef == RoR2Content.Survivors.Croco)
                {
                    healthMultiplier += 0.5f;
                }
                else if (survivorDef == RoR2Content.Survivors.Engi)
                {
                    damageReduction = 0f;

                }
                else if (survivorDef == RoR2Content.Survivors.Huntress)
                {

                }
                else if (survivorDef == RoR2Content.Survivors.Loader)
                {
                    healthMultiplier += 0.5f;
                }
                else if (survivorDef == RoR2Content.Survivors.Mage) //secondary
                {
                    damageReduction += 0.2f;
                }
                else if (survivorDef == RoR2Content.Survivors.Merc)
                {

                }
                else if (survivorDef == RoR2Content.Survivors.Toolbot)
                {

                }
                else if (survivorDef == RoR2Content.Survivors.Treebot)
                {

                }
                else if (characterBody.bodyIndex == hereticBodyIndex)
                {

                }
                else if (survivorDef == DLC1Content.Survivors.VoidSurvivor)
                {

                }
                else if (survivorDef == DLC1Content.Survivors.Railgunner)
                {

                }
                characterBody.baseDamage *= (1f - damageReduction);
                characterBody.baseMaxHealth *= healthMultiplier;
                characterBody.levelMaxHealth = characterBody.baseMaxHealth * 0.3f;
            }
        }
    }
}