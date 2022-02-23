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
using UnityEngine.Networking;
using static UniqueItemsArtifact.InstanceTracking;
using static UniqueItemsArtifact.Availability;
using System;
using System.Runtime.CompilerServices;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace UniqueItemsArtifact
{
    [BepInPlugin("com.DestroyedClone.UniqueItemArtifact", "Unique Item Artifact", "0.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(new string[] {
        nameof(ArtifactAPI),
        nameof(DirectorAPI)

    })]
    public class UIAMain : BaseUnityPlugin
    {
        /* TODO: 
         * Add equipment to blacklist
         * Up Purcahse Cost
         * Reduce Chest amount
         * List Seperation:
         *  Store list for pickupIndex in world (needed)
         *      Upon spawning, remove from drop list
         *      On stage end, if this list is occupied, re-add to drop list
         *  Store list for pickupIndex taken (current)
         *      Upon pickup, remove from drop list
         *      Upon removal, add to drop list
         * Full code cleanup + Performance
         * Networking
         * Code Cleanup
         */

        public static ArtifactDef uniqueArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ArtifactDef EvolRef = Resources.Load<ArtifactDef>("artifactdefs/MonsterTeamGainsItems");

        public static List<PickupIndex> existingPickupIndices = new List<PickupIndex>();
        public static List<PickupIndex> takenPickupIndices = new List<PickupIndex>();

        public readonly int multCommon = 5;
        public readonly int multUncommon = 3;
        public readonly int multLegendary = 1;
        public readonly int multLunar = 1;
        public readonly int multBoss = 1;

        public static List<PickupIndex> cachedTier1DropList;
        public static List<PickupIndex> cachedTier2DropList;
        public static List<PickupIndex> cachedTier3DropList;

        public readonly float multCostCommon = 2f;
        public readonly float multCostUncommon = 1f;

        public void Awake()
        {
            SetupArtifact();
            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;
        }

        public int AdjustItemCount(ItemIndex itemIndex)
        {
            switch (ItemCatalog.GetItemDef(itemIndex).tier)
            {
                case ItemTier.Tier1:
                    return multCommon;
                case ItemTier.Tier2:
                    return multUncommon;
                case ItemTier.Tier3:
                    return multLegendary;
                case ItemTier.Lunar:
                    return multLunar;
                case ItemTier.Boss:
                    return multBoss;
                case ItemTier.NoTier:
                    break;
            }
            return 1;
        }

        public static void SetupArtifact()
        {
            uniqueArtifactDef.nameToken = "Artifact of Uniquety";
            uniqueArtifactDef.descriptionToken = "Items are unique.";
            uniqueArtifactDef.smallIconDeselectedSprite = EvolRef.smallIconDeselectedSprite;
            uniqueArtifactDef.smallIconSelectedSprite = EvolRef.smallIconSelectedSprite;
            ArtifactAPI.Add(uniqueArtifactDef);
        }
        private void RunArtifactManager_onArtifactEnabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (artifactDef != uniqueArtifactDef)
            {
                return;
            }
            On.RoR2.MultiShopController.Awake += Track_MultiShopCrontroller_Add;
            On.RoR2.MultiShopController.OnDestroy += Track_MultiShopCrontroller_Remove;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased += DisableTerminalOnPurchase;
            On.RoR2.ChestBehavior.Awake += Track_ChestBehavior;

            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += Inventory_RemoveItem_ItemIndex_int;
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.CostTypeDef.PayCost += CostTypeDef_PayCost;

            On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards; //doesnt work

            On.RoR2.MultiShopController.CreateTerminals += PreventTerminalsIfNoItems;

            SceneDirector.onGenerateInteractableCardSelection += RemoveInteractableCardsIfNoPickupAvailable;

            On.RoR2.ShopTerminalBehavior.GenerateNewPickupServer += ShopTerminalBehavior_GenerateNewPickupServer;

            On.RoR2.ArenaMissionController.EndRound += ArenaMissionController_EndRound;
            On.RoR2.ScavengerItemGranter.Start += ScavengerItemGranter_Start;

            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void Run_onRunStartGlobal(Run run)
        {
            cachedTier1DropList = new List<PickupIndex>(run.availableTier1DropList);
            cachedTier2DropList = new List<PickupIndex>(run.availableTier2DropList);
            cachedTier3DropList = new List<PickupIndex>(run.availableTier3DropList);
        }

        private void ScavengerItemGranter_Start(On.RoR2.ScavengerItemGranter.orig_Start orig, ScavengerItemGranter self)
        {
            Inventory component = base.GetComponent<Inventory>();
            if (ItemTierAvailable(ItemTier.Tier1))
            {
                List<PickupIndex> list = cachedTier1DropList.Where(new Func<PickupIndex, bool>(PickupIsNonBlacklistedItem)).ToList<PickupIndex>();
                self.GrantItems(component, list, self.tier1Types, self.tier1StackSize);
            }
            if (ItemTierAvailable(ItemTier.Tier2))
            {
                List<PickupIndex> list2 = cachedTier2DropList.Where(new Func<PickupIndex, bool>(PickupIsNonBlacklistedItem)).ToList<PickupIndex>();
                self.GrantItems(component, list2, self.tier2Types, self.tier2StackSize);
            }
            if (ItemTierAvailable(ItemTier.Tier3))
            {
                List<PickupIndex> list3 = cachedTier3DropList.Where(new Func<PickupIndex, bool>(PickupIsNonBlacklistedItem)).ToList<PickupIndex>();
                self.GrantItems(component, list3, self.tier3Types, self.tier3StackSize);
            }
            List<PickupIndex> availableEquipmentDropList = Run.instance.availableEquipmentDropList;
            if (self.overwriteEquipment || component.currentEquipmentIndex == EquipmentIndex.None)
            {
                component.GiveRandomEquipment();
            }
        }

        public static bool PickupIsNonBlacklistedItem(PickupIndex pickupIndex)
        {
            var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
            return itemDef.DoesNotContainTag(ItemTag.AIBlacklist);
        }

        private void ArenaMissionController_EndRound(On.RoR2.ArenaMissionController.orig_EndRound orig, ArenaMissionController self)
        {
            GameObject cachedRewardPosition = self.rewardSpawnPosition;
            self.rewardSpawnPosition = null;
            int participatingPlayerCount = Run.instance.participatingPlayerCount;
            if (participatingPlayerCount != 0 && self.rewardSpawnPosition)
            {
                List<PickupIndex> list = Run.instance.availableTier1DropList;
                if (self.currentRound > 4)
                {
                    list = Run.instance.availableTier2DropList;
                }
                if (self.currentRound == self.totalRoundsMax)
                {
                    list = Run.instance.availableTier3DropList;
                }
                if (list.Count > 0)
                {
                    PickupIndex pickupIndex = self.rng.NextElementUniform<PickupIndex>(list);
                    int num = participatingPlayerCount;
                    float angle = 360f / (float)num;
                    Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    int k = 0;
                    while (k < num)
                    {
                        PickupDropletController.CreatePickupDroplet(pickupIndex, self.rewardSpawnPosition.transform.position, vector);
                        k++;
                        vector = rotation * vector;
                    }
                }
            }
            orig(self);
            self.rewardSpawnPosition = cachedRewardPosition;
        }

        private void ShopTerminalBehavior_GenerateNewPickupServer(On.RoR2.ShopTerminalBehavior.orig_GenerateNewPickupServer orig, ShopTerminalBehavior self)
        {
            bool tierAvailable = ItemTierAvailable(self.itemTier);

            if (tierAvailable)
            {
                orig(self);
            }
        }

        private void RemoveInteractableCardsIfNoPickupAvailable(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            if (Run.instance)
            {
                dccs.RemoveCardsThatFailFilter(new Predicate<DirectorCard>(OnGenerateInteractableCardSelection));
            }
        }

        internal static bool OnGenerateInteractableCardSelection(DirectorCard card)
		{
			GameObject prefab = card.spawnCard.prefab;
            var spawnCard = card.spawnCard;

            InteractableSpawnCard interactableSpawnCard(string name) { return Resources.Load<InteractableSpawnCard>($"spawncards/interactablespawncard/{name}"); }
            bool isCard(string name) { return spawnCard == interactableSpawnCard(name); }

            ItemTier[] tierWhiteGreen = new ItemTier[] { ItemTier.Tier1, ItemTier.Tier2 };
            ItemTier[] tierWhiteGreenRed = new ItemTier[] { ItemTier.Tier1, ItemTier.Tier2, ItemTier.Tier3 };
            ItemTier[] tierGreenRed = new ItemTier[] { ItemTier.Tier2, ItemTier.Tier3 };

            //bool conditionsToRemove = prefab.GetComponent<ShopTerminalBehavior>() || prefab.GetComponent<MultiShopController>() || prefab.GetComponent<ScrapperController>();
            bool conditionsToRemove = prefab.GetComponent<ShopTerminalBehavior>() || prefab.GetComponent<ScrapperController>();

            if (isCard("isccasinochest")) // white/green/red
            {
                conditionsToRemove = !ItemTierAvailable(tierWhiteGreenRed);
            }
            else if (isCard("isccategorychestdamage")) // white/green
            {
                conditionsToRemove = !ItemTagAvailable(tierWhiteGreenRed, ItemTag.Damage);
                //isAvailable = ItemTierAvailable(tierWhiteGreenRed);
                //if (isAvailable)
                //conditionsToRemove = !(ItemTierAvailable(tierWhiteGreen) && ItemTagAvailable(tierWhiteGreen, ItemTag.Damage));
            }
            else if (isCard("isccategorychesthealing")) // white/green
            {
                conditionsToRemove = !ItemTagAvailable(tierWhiteGreenRed, ItemTag.Healing);
            }
            else if (isCard("isccategorychestutility")) // white/green
            {
                conditionsToRemove = !ItemTagAvailable(tierWhiteGreenRed, ItemTag.Utility);
            }
            else if (isCard("iscchest1") || isCard("iscchest1stealthed") || isCard("iscscavbackpack") || isCard("iscshrinechance")) // white/green/red
            {
                conditionsToRemove = !ItemTierAvailable(tierWhiteGreenRed);
            }
            else if (isCard("iscchest2") || isCard("isclockbox")) // green/red
            {
                conditionsToRemove = !ItemTierAvailable(tierGreenRed);
            }
            else if (isCard("iscgoldchest")) // red
            {
                conditionsToRemove = !ItemTierAvailable(ItemTier.Tier3);
            }
            else if (isCard("isclunarchest"))
            {
                conditionsToRemove = !ItemTierAvailable(ItemTier.Lunar);
            }
            else if (isCard("isctripleshop") || isCard("isctripleshoplarge")) // white/green
            {
                conditionsToRemove = !ItemTierAvailable(tierWhiteGreen);
            }
            conditionsToRemove = !conditionsToRemove;
            //Debug.Log($"{(conditionsToRemove ? "REMOVE" : "KEEP")}: {spawnCard.prefab.name}");
            return conditionsToRemove;
		}

        private void PreventTerminalsIfNoItems(On.RoR2.MultiShopController.orig_CreateTerminals orig, MultiShopController self)
        {
            if (!self.doEquipmentInstead && ItemTierAvailable(self.itemTier)) orig(self);
        }

        private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
        {
            if (self.dropPosition)
            {
                List<PickupIndex> list = Run.instance.availableTier2DropList;
                if (self.forceTier3Reward)
                {
                    list = Run.instance.availableTier3DropList;
                }
                PickupIndex pickupIndex = self.rng.NextElementUniform<PickupIndex>(list);
                int num = 1 + self.bonusRewardCount;
                float angle = 360f / (float)num;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                int i = 0;
                while (i < num)
                {
                    PickupIndex pickupIndex2 = pickupIndex;
                    if (self.bossDrops.Count > 0 && self.rng.nextNormalizedFloat <= self.bossDropChance)
                    {
                        pickupIndex2 = self.rng.NextElementUniform<PickupIndex>(self.bossDrops);
                        self.bossDrops.Remove(pickupIndex2);
                    }
                    PickupDropletController.CreatePickupDroplet(pickupIndex2, self.dropPosition.position, vector);
                    i++;
                    vector = rotation * vector;
                }
            }
        }

        private void DisableTerminalOnPurchase(On.RoR2.ShopTerminalBehavior.orig_SetHasBeenPurchased orig, ShopTerminalBehavior self, bool newHasBeenPurchased)
        {
            orig(self, newHasBeenPurchased);
            if (newHasBeenPurchased && self.NetworkhasBeenPurchased == newHasBeenPurchased)
            {
                var com = self.GetComponent<PurchaseInteraction>();
                com.costType = CostTypeIndex.VolatileBattery;
                com.Networkcost = 2;
            }
        }

        private CostTypeDef.PayCostResults CostTypeDef_PayCost(On.RoR2.CostTypeDef.orig_PayCost orig, CostTypeDef self, int cost, Interactor activator, GameObject purchasedObject, Xoroshiro128Plus rng, ItemIndex avoidedItemIndex)
        {
            var original = orig(self, cost, activator, purchasedObject, rng, avoidedItemIndex);
            //var copyList = new List<ItemIndex>(original.itemsTaken);
            if (self.itemTier != ItemTier.NoTier)
            {
                CostTypeDef.PayCostResults payCostResults = new CostTypeDef.PayCostResults();
                for (int i = 0; i < cost; i++)
                {
                    payCostResults.itemsTaken.Add(original.itemsTaken[0]);
                }
                /*for (int i = 0; i < original.itemsTaken.Count; i++)
                {
                    original.itemsTaken[i] = original.itemsTaken[0];
                }*/
                return payCostResults;
            }
            return original;
        }

        private void RunArtifactManager_onArtifactDisabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef != uniqueArtifactDef)
            {
                return;
            }
            On.RoR2.MultiShopController.Awake -= Track_MultiShopCrontroller_Add;
            On.RoR2.MultiShopController.OnDestroy -= Track_MultiShopCrontroller_Remove;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased -= DisableTerminalOnPurchase;
            On.RoR2.ChestBehavior.Awake -= Track_ChestBehavior;

            On.RoR2.Inventory.GiveItem_ItemIndex_int -= Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int -= Inventory_RemoveItem_ItemIndex_int;
            On.RoR2.PurchaseInteraction.Awake -= PurchaseInteraction_Awake;
            On.RoR2.CostTypeDef.PayCost -= CostTypeDef_PayCost;

            On.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards; //doesnt work

            On.RoR2.MultiShopController.CreateTerminals -= PreventTerminalsIfNoItems;

            SceneDirector.onGenerateInteractableCardSelection -= RemoveInteractableCardsIfNoPickupAvailable;

            On.RoR2.ShopTerminalBehavior.GenerateNewPickupServer -= ShopTerminalBehavior_GenerateNewPickupServer;

            On.RoR2.ArenaMissionController.EndRound -= ArenaMissionController_EndRound;
            On.RoR2.ScavengerItemGranter.Start -= ScavengerItemGranter_Start;

            Run.onRunStartGlobal -= Run_onRunStartGlobal;
        }

        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            switch (self.costType)
            {
                case CostTypeIndex.ArtifactShellKillerItem:
                    break;
                case CostTypeIndex.WhiteItem:
                    self.Networkcost = multCommon;
                    break;
                case CostTypeIndex.GreenItem:
                    self.Networkcost = multUncommon;
                    break;
                case CostTypeIndex.BossItem:
                    self.Networkcost = multBoss;
                    break;
                case CostTypeIndex.RedItem:
                    self.Networkcost = multCommon;
                    break;
            }
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            var newCount = AdjustItemCount(itemIndex);
            if (count > 0 && count < newCount)
            {
                count = newCount;
            }
            orig(self, itemIndex, count);
            ModifyDropList(itemIndex, false);
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            var newCount = AdjustItemCount(itemIndex);
            if (count > 0 && count < newCount)
            {
                count = newCount;
            }
            orig(self, itemIndex, count);
            ModifyDropList(itemIndex, true);

        }

        private void ModifyDropList(ItemIndex itemIndex, bool add)
        {
            Run run = RoR2.Run.instance;
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            ItemTier itemTier = itemDef.tier;
            if (itemDef.hidden || itemTier == ItemTier.NoTier)
            {
                return;
            }


            var pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);

            bool runHasItem = run.availableItems.array[(int)itemIndex];
            bool tier1Has = run.availableTier1DropList.Contains(pickupIndex);
            bool tier2Has = run.availableTier2DropList.Contains(pickupIndex);
            bool tier3Has = run.availableTier3DropList.Contains(pickupIndex);
            bool tierBossHas = run.availableBossDropList.Contains(pickupIndex);
            bool tierLunarHas = run.availableLunarDropList.Contains(pickupIndex);


            if (add)
            {
                if (!runHasItem)
                {
                    //run.availableItems.array[(int)itemIndex] = true;
                    run.availableItems.Add(itemIndex);
                    switch (itemTier)
                    {
                        case ItemTier.Tier1:
                            if (!tier1Has)
                                run.availableTier1DropList.Add(pickupIndex);
                            break;
                        case ItemTier.Tier2:
                            if (!tier2Has)
                                run.availableTier2DropList.Add(pickupIndex);
                            break;
                        case ItemTier.Tier3:
                            if (!tier3Has)
                                run.availableTier3DropList.Add(pickupIndex);
                            break;
                        case ItemTier.Lunar:
                            if (!tierLunarHas)
                                run.availableLunarDropList.Add(pickupIndex);
                            break;
                        case ItemTier.Boss:
                            if (!tierBossHas)
                                run.availableBossDropList.Add(pickupIndex);
                            break;
                    }
                }
            } else
            {
                if (runHasItem)
                {
                    //run.availableItems.array[(int)itemIndex] = false;
                    run.availableItems.Remove(itemIndex);
                    switch (itemTier)
                    {
                        case ItemTier.Tier1:
                            if (tier1Has)
                                run.availableTier1DropList.Remove(pickupIndex);
                            break;
                        case ItemTier.Tier2:
                            if (tier2Has)
                                run.availableTier2DropList.Remove(pickupIndex);
                            break;
                        case ItemTier.Tier3:
                            if (tier3Has)
                                run.availableTier3DropList.Remove(pickupIndex);
                            break;
                        case ItemTier.Lunar:
                            if (tierLunarHas)
                                run.availableLunarDropList.Remove(pickupIndex);
                            break;
                        case ItemTier.Boss:
                            if (tierBossHas)
                                run.availableBossDropList.Remove(pickupIndex);
                            break;
                    }
                }
            }
            run.BuildDropTable();
            PickupDropTable.RegenerateAll(run);
            bool tier1Available = ItemTierAvailable(ItemTier.Tier1);
            bool tier2Available = ItemTierAvailable(ItemTier.Tier2);
            bool tier3Available = ItemTierAvailable(ItemTier.Tier3);
            bool noTierAvailable = !tier1Available && !tier2Available && !tier3Available;
            //Debug.Log($"Tier Availability: {tier1Available} {tier2Available} {tier3Available}");

            void DestroyAndSpawnGold(GameObject gameObject, int count = 0)
            {
                if (gameObject)
                {
                    GameObject gameObject9 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/BonusMoneyPack"), gameObject.transform.position, UnityEngine.Random.rotation);
                    gameObject9.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
                    for (int i = 0; i < count; i++)
                    {
                        NetworkServer.Spawn(gameObject9);
                    }
                    Destroy(gameObject);
                }
            }

            void HandleTerminalBehaviors()
            {
                foreach (var shopTerminalBehavior in InstanceTracker.GetInstancesList<ShopTerminalBehavior>())
                {
                    if (noTierAvailable)
                    {
                        if (shopTerminalBehavior.GetComponent<PurchaseInteraction>().Networkavailable)
                            DestroyAndSpawnGold(shopTerminalBehavior.gameObject, 3);
                    }
                    else
                    if (shopTerminalBehavior.pickupIndex == pickupIndex)
                    {
                        shopTerminalBehavior.GenerateNewPickupServer();
                        shopTerminalBehavior.UpdatePickupDisplayAndAnimations();
                    }
                }
            }
            HandleTerminalBehaviors();

            void HandleChestBehaviors()
            {
                List<ChestBehavior> chestBehaviorsToRemove = new List<ChestBehavior>();
                foreach (var chestBehavior in InstanceTracker.GetInstancesList<ChestBehavior>())
                {
                    bool notPurchased = chestBehavior.GetComponent<PurchaseInteraction>().Networkavailable;
                    void DestroyGold()
                    {
                        if (notPurchased)
                        {
                            DestroyAndSpawnGold(chestBehavior.gameObject, 1);
                            chestBehaviorsToRemove.Add(chestBehavior);
                        }
                    }
                    if (noTierAvailable)
                    {
                        DestroyGold();
                    }
                    else
                    {
                        chestBehavior.tier1Chance *= (tier1Available ? 1 : 0);
                        chestBehavior.tier2Chance *= (tier2Available ? 1 : 0);
                        chestBehavior.tier3Chance *= (tier3Available ? 1 : 0);

                        var availableItemTiers = new List<ItemTier>();
                        if (chestBehavior.tier1Chance != 0) availableItemTiers.Add(ItemTier.Tier1);
                        if (chestBehavior.tier2Chance != 0) availableItemTiers.Add(ItemTier.Tier2);
                        if (chestBehavior.tier3Chance != 0) availableItemTiers.Add(ItemTier.Tier3);
                        bool itemTagAvailable = chestBehavior.requiredItemTag == ItemTag.Any ? true : Availability.ItemTagAvailable(availableItemTiers.ToArray(), chestBehavior.requiredItemTag);

                        if (!itemTagAvailable || (chestBehavior.tier1Chance == 0 && chestBehavior.tier2Chance == 0 && chestBehavior.tier3Chance == 0))
                        {
                            DestroyGold();
                        }
                        else
                        {
                            if (chestBehavior.dropPickup == pickupIndex)
                                chestBehavior.Start();
                        }
                    }
                }
                if (chestBehaviorsToRemove.Count > 0)
                {
                    foreach (var cb in chestBehaviorsToRemove)
                    {
                        if (cb)
                        {
                            InstanceTracker.Remove(cb);
                        }
                    }
                }
            }
            HandleChestBehaviors();
        }
    }
}
