using RoR2;
using System;
using static BetterEquipmentDroneUse.Main;
using System.Collections.Generic;

namespace BetterEquipmentDroneUse
{
    public class SystemInitializers
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

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.EquipmentCatalog))]
        public static void SetupDictionary()
        {
            DroneModeDictionary = new Dictionary<EquipmentIndex, DroneMode>()
            {
                // If there are enemies on the map, but not necessarily any priority targets. //
                [RoR2Content.Equipment.CommandMissile.equipmentIndex] = DroneMode.EnemyOnMap,
                [RoR2Content.Equipment.Meteor.equipmentIndex] = DroneMode.EnemyOnMap,

                [RoR2Content.Equipment.Blackhole.equipmentIndex] = DroneMode.PriorityTarget,
                [RoR2Content.Equipment.BFG.equipmentIndex] = DroneMode.PriorityTarget,
                [RoR2Content.Equipment.Lightning.equipmentIndex] = DroneMode.PriorityTarget,
                [RoR2Content.Equipment.CrippleWard.equipmentIndex] = DroneMode.PriorityTarget,

                [RoR2Content.Equipment.Jetpack.equipmentIndex] = DroneMode.Evade,
                [RoR2Content.Equipment.GainArmor.equipmentIndex] = DroneMode.Evade,
                [RoR2Content.Equipment.Tonic.equipmentIndex] = DroneMode.Evade,

                [RoR2Content.Equipment.GoldGat.equipmentIndex] = DroneMode.GoldGat,

                [RoR2Content.Equipment.PassiveHealing.equipmentIndex] = DroneMode.PassiveHealing,

                [RoR2Content.Equipment.Gateway.equipmentIndex] = DroneMode.Gateway,

                [RoR2Content.Equipment.Cleanse.equipmentIndex] = DroneMode.Cleanse,

                [RoR2Content.Equipment.Saw.equipmentIndex] = DroneMode.Saw,

                [RoR2Content.Equipment.Recycle.equipmentIndex] = DroneMode.Recycle,

                [RoR2Content.Equipment.Fruit.equipmentIndex] = DroneMode.Fruit,

                [RoR2Content.Equipment.BurnNearby.equipmentIndex] = DroneMode.Snuggle,
                [RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex] = DroneMode.Snuggle,

                [RoR2Content.Equipment.Scanner.equipmentIndex] = DroneMode.Scan,
            };
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