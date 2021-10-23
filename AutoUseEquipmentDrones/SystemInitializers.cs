using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.RoR2Content.Equipment;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EntityStates;
using JetBrains.Annotations;
using RoR2.Navigation;
using UnityEngine.AI;
using EntityStates.GoldGat;
using System.Security;
using System.Security.Permissions;
using static BetterEquipmentDroneUse.Methods;
using EntityStates.AI;
using RoR2.DirectionalSearch;
using RoR2.Orbs;
using RoR2.Projectile;
using BetterEquipmentDroneUse;
using static BetterEquipmentDroneUse.BetterEquipmentDroneUsePlugin;

namespace BetterEquipmentDroneUse
{
    public  class SystemInitializers
    {
        [RoR2.SystemInitializer(dependencies: typeof(RoR2.ItemCatalog))]
        private static void CacheWhitelistedItems()
        {
            //_logger.LogMessage("Caching whitelisted items for Recycler.");
            var testStringArray = Recycler_Items.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (ItemCatalog.FindItemIndex(stringToTest) == ItemIndex.None) { continue; }
                    allowedItemIndices.Add(ItemCatalog.FindItemIndex(stringToTest));
                    //_logger.LogMessage("Adding whitelisted item: " + stringToTest);
                }
            }
            _logger.LogMessage(allowedItemIndices);
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.EquipmentCatalog))]
        private static void CacheWhitelistedEquipment()
        {
            //_logger.LogMessage("Caching whitelisted EQUIPMENT for Recycler.");
            var testStringArray = Recycler_Equipment.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (EquipmentCatalog.FindEquipmentIndex(stringToTest) == EquipmentIndex.None) { continue; }
                    allowedEquipmentIndices.Add(EquipmentCatalog.FindEquipmentIndex(stringToTest));
                    //_logger.LogMessage("Adding whitelisted equipment: " + stringToTest);
                }
            }
        }

        [RoR2.SystemInitializer(dependencies: new Type[] { typeof(RoR2.PickupCatalog), typeof(RoR2.ItemCatalog), typeof(RoR2.EquipmentCatalog) })]
        private static void CachePickupIndices()
        {
            foreach (var itemIndex in allowedItemIndices)
            {
                if (PickupCatalog.FindPickupIndex(itemIndex) != PickupIndex.none)
                    allowedPickupIndices.Add(PickupCatalog.FindPickupIndex(itemIndex));
            }
            foreach (var equipmentIndex in allowedEquipmentIndices)
            {
                if (PickupCatalog.FindPickupIndex(equipmentIndex) != PickupIndex.none)
                    allowedPickupIndices.Add(PickupCatalog.FindPickupIndex(equipmentIndex));
            }
            _logger.LogMessage("Listing allowed pickups:");
            foreach (var pickupIndex in allowedPickupIndices)
            {
                var def = PickupCatalog.GetPickupDef(pickupIndex).internalName;
                _logger.LogMessage(def);
            }
            _logger.LogMessage("Done.");
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.ChestRevealer))]
        private static void CacheAllowedChestRevealerTypes()
        {
            allowedTypesToScan = ChestRevealer.typesToCheck;
        }

        [RoR2.SystemInitializer(dependencies: typeof(BodyCatalog))]
        private static void CachedBodyIndex()
        {
            EquipmentDroneBodyIndex = BodyCatalog.FindBodyIndex("EquipmentDroneBody");
        }
    }
}
