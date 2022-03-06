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

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace BanditItemlet
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
        public static BodyIndex banditBodyIndex;
        public static BepInEx.Logging.ManualLogSource _logger;
        public static ConfigEntry<bool> cfgLoop;
        public static DirectorCard[] survivorDirectorCards;
        public static CharacterSpawnCard[] survivorSpawnCards;
        public static DirectorAPI.DirectorCardHolder[] directorCardHolders;
        public static ConfigEntry<float> cfgCommonMult;
        public static ConfigEntry<float> cfgUncommonMult;
        public static ConfigEntry<float> cfgLegendaryMult;
        public static BodyIndex hereticBodyIndex;
        public static ConfigEntry<float> cfgHeresyScale;
        public static ConfigEntry<bool> cfgHereticEnable;
        public static BodyIndex[] survivorBodyIndices;
        public static ItemDef minusOnePercentDamage;

        public void Start()
        {
            _logger = Logger;
            CreateItems();
            cfgLoop = Config.Bind("Spawning","Only Loop",false,"If true, then survivors will only spawn after looping.");
            cfgCommonMult = Config.Bind("Items", "Common", 2f, "The value, multiplied by the stage count, of the amount of items given.");
            cfgUncommonMult = Config.Bind("Items", "Uncommon", 0.4f, "The value, multiplied by the stage count, of the amount of items given.");
            cfgLegendaryMult = Config.Bind("Items", "Legendary", 0.3f, "The value, multiplied by the stage count, of the amount of items given.");
            //cfgHereticEnable = Config.Bind("Spawning", "Enable Heretic", )
            cfgHeresyScale = Config.Bind("Items", "Heretic + Heresy Set", 0.5f, "");
            SetupDirectorCards();
            DirectorAPI.MonsterActions += AddSurvivors;
            On.RoR2.DirectorCore.TrySpawnObject += DirectorCore_TrySpawnObject;
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
            minusOnePercentDamage = new ItemDef()
            {
                canRemove = false,
                descriptionToken = "",
                hidden = true,
                loreToken = "",
                nameToken = "SAE_ITEM_REDUCE_DAMAGE_ONE_PERCENT",
                pickupToken = "",
                tags = new ItemTag[] { },
                tier = ItemTier.NoTier,
            };
            ItemAPI.Add(new CustomItem(minusOnePercentDamage, new ItemDisplayRule[] { }));
        }

        private GameObject DirectorCore_TrySpawnObject(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            var original = orig(self, directorSpawnRequest);
            if (directorSpawnRequest.spawnCard && directorSpawnRequest.spawnCard.prefab)
            {
                var characterMaster = directorSpawnRequest.spawnCard.prefab.GetComponent<CharacterMaster>();
                if (characterMaster)
                {
                    var bodyIndex = characterMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                    if (survivorBodyIndices.Contains(bodyIndex))
                    {
                        directorSpawnRequest.teamIndexOverride = TeamIndex.Neutral;
                    }
                }
            }
            return original;
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
                            _logger.LogMessage("Added to directorapi "+ directorCardHolder.Card.spawnCard.prefab.name);
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
            hereticBodyIndex = BodyCatalog.FindBodyIndex("HereticBody");
            List<DirectorCard> sdc = new List<DirectorCard>();
            List<CharacterSpawnCard> spc = new List<CharacterSpawnCard>();
            List<DirectorAPI.DirectorCardHolder> dch = new List<DirectorAPI.DirectorCardHolder>();

            List<CharacterMaster> characterMasters = new List<CharacterMaster>();
            var bodyIndexList = new List<BodyIndex>();

            foreach (var survivorDef in SurvivorCatalog.allSurvivorDefs)
            {
                characterMasters.Add(MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(BodyCatalog.FindBodyIndex(survivorDef.bodyPrefab))).GetComponent<CharacterMaster>());
            }

            foreach (var characterMaster in characterMasters)
            {
                var characterBody = characterMaster.bodyPrefab.GetComponent<CharacterBody>();
                bodyIndexList.Add(characterBody.bodyIndex);

                if (characterMaster && characterMaster.inventory)
                {
                    var itemGranter = characterMaster.gameObject.AddComponent<ItemSetterUpper>();
                    itemGranter.inventory = characterMaster.inventory;
                    itemGranter.characterMaster = characterMaster;
                    itemGranter.survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(itemGranter.characterMaster.bodyPrefab);
                }

                CharacterSpawnCard survivorSpawnCard = new CharacterSpawnCard()
                {
                    directorCreditCost = 150,
                    eliteRules = SpawnCard.EliteRules.Default,
                    //equipmentToGrant = ,
                    forbiddenAsBoss = true,
                    forbiddenFlags = RoR2.Navigation.NodeFlags.None,
                    hullSize = characterBody.hullClassification,
                    nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground,
                    noElites = true,
                    occupyPosition = false,
                    sendOverNetwork = true,
                    prefab = characterMaster.gameObject
                };
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
                throw new System.NotImplementedException();
            }

            public void Start()
            {
                if (inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
                    return;
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
                    if (body.bodyIndex == hereticBodyIndex)
                    {
                        GiveHeresyItemsIfMissing();
                    }
                }

                //Reduce
            }

            public void GiveHeresyItemsIfMissing()
            {
                int requiredCount = Mathf.RoundToInt(Run.instance.stageClearCount * cfgHeresyScale.Value);
                foreach (ItemDef itemDef in new ItemDef[] {LunarPrimaryReplacement,LunarSecondaryReplacement,LunarUtilityReplacement,LunarSpecialReplacement})
                {
                    var difference = requiredCount - inventory.GetItemCount(itemDef);
                    if (difference > 0)
                    {
                        inventory.GiveItem(itemDef, difference);
                    }
                }
            }

            public void FixedUpdate()
            {
                if (replaceBaseStats)
                {
                    var body = characterMaster.GetBody();
                    if (body)
                    {
                        replaceBaseStats = false;
                        ModifyBaseStats(body);
                    }
                }
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
                } else if (survivorDef == RoR2Content.Survivors.Captain) //utility
                {
                    damageReduction += 0.2f;
                } else if (survivorDef == RoR2Content.Survivors.Commando)
                {

                } else if (survivorDef == RoR2Content.Survivors.Croco)
                {
                    healthMultiplier += 0.5f;
                } else if (survivorDef == RoR2Content.Survivors.Engi)
                {
                    damageReduction = 0f;

                } else if (survivorDef == RoR2Content.Survivors.Huntress)
                {

                } else if (survivorDef == RoR2Content.Survivors.Loader)
                {
                    healthMultiplier += 0.5f;
                } else if (survivorDef == RoR2Content.Survivors.Mage) //secondary
                {
                    damageReduction += 0.2f;
                } else if (survivorDef == RoR2Content.Survivors.Merc)
                {

                } else if (survivorDef == RoR2Content.Survivors.Toolbot)
                {

                } else if (survivorDef == RoR2Content.Survivors.Treebot)
                {

                } else if (characterBody.bodyIndex == hereticBodyIndex)
                {

                }
                characterBody.baseDamage *= (1f - damageReduction);
                characterBody.baseMaxHealth *= healthMultiplier;
                characterBody.levelMaxHealth = characterBody.baseMaxHealth * 0.3f;
            }
        }
    }
}
