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
using System;
using System.Runtime.CompilerServices;

namespace UniqueItemsArtifact
{
    public class Availability
    {

        public static bool ItemTierAvailable(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    return Run.instance.availableTier1DropList.Count > 0;
                case ItemTier.Tier2:
                    return Run.instance.availableTier2DropList.Count > 0;
                case ItemTier.Tier3:
                    return Run.instance.availableTier3DropList.Count > 0;
                case ItemTier.Lunar:
                    return Run.instance.availableLunarDropList.Count > 0;
                default:
                    return true;
            }
        }

        public static bool ItemTierAvailable(ItemTier[] itemTiers)
        {
            bool isAvailable = true;
            foreach (var itemTier in itemTiers)
            {
                isAvailable &= ItemTierAvailable(itemTier);
            }
            return isAvailable;
        }

        public static bool ItemTagAvailable(ItemTier[] itemTiers, ItemTag itemTag)
        {
            bool check(PickupIndex pickupIndex)
            {
                var itemDef = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickupIndex).itemIndex);
                if (itemDef.ContainsTag(itemTag))
                {
                    //Debug.Log($"{itemTag} found for {itemDef.tier} {itemDef.nameToken}");
                    return itemDef.ContainsTag(itemTag);
                }
                return false;
            }

            bool isAvailable = false;


            if (!isAvailable && itemTiers.Contains(ItemTier.Tier1))
                foreach (PickupIndex pickupIndex in Run.instance.availableTier1DropList)
                {
                    if (check(pickupIndex)) { isAvailable = true; break; }
                }
            if (!isAvailable && itemTiers.Contains(ItemTier.Tier2))
                foreach (PickupIndex pickupIndex in Run.instance.availableTier2DropList)
                {
                    if (check(pickupIndex)) { isAvailable = true; break; }
                }
            if (!isAvailable && itemTiers.Contains(ItemTier.Tier3))
                foreach (PickupIndex pickupIndex in Run.instance.availableTier3DropList)
                {
                    if (check(pickupIndex)) { isAvailable = true; break; }
                }
            if (!isAvailable && itemTiers.Contains(ItemTier.Lunar))
                foreach (PickupIndex pickupIndex in Run.instance.availableLunarDropList)
                {
                    if (check(pickupIndex)) { isAvailable = true; break; }
                }
            if (!isAvailable && itemTiers.Contains(ItemTier.Boss))
                foreach (PickupIndex pickupIndex in Run.instance.availableBossDropList)
                {
                    if (check(pickupIndex)) { isAvailable = true; break; }
                }

            return isAvailable;
        }
    }
}
