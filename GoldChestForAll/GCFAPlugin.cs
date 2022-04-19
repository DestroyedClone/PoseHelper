using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace GoldChestForAll
{
    [BepInPlugin("com.DestroyedClone.GoldChestForAll", "GoldChestForAll", "1.0.11")]
    public class GCFAPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> cfgCostMultiplier { get; set; }
        public static ConfigEntry<bool> CfgIntent { get; set; }

        public void Start()
        {
            cfgCostMultiplier = Config.Bind("Default", "Gold Chest Cost Multiplier", 1.00f, "Multiply the costs of gold chests. Intended for balance, but you can just set it to '1' if you want it unchanged.");
            CfgIntent = Config.Bind("Default", "Only Abyssal Depths and Sundered Grove", true, "If true, then only the guaranteed chest on Sundered Grove and Abyssal Depths will have its amount increased.");

            On.RoR2.ChestBehavior.ItemDrop += DuplicateDrops;

            if (cfgCostMultiplier.Value != 1f)
            {
                On.RoR2.PurchaseInteraction.Awake += MultiplyChestCost;
            }
        }


        private bool IsValidScene(Transform chestTransform)
        {
            if (CfgIntent.Value)
            {
                bool parentIsRootJungle = chestTransform.parent && chestTransform.parent.name == "HOLDER: Newt Statues and Preplaced Chests";
                bool parentIsDampCaveSimple = chestTransform.parent
                    && chestTransform.parent.parent
                    && chestTransform.parent.parent.name == "GROUP: Large Treasure Chests";
                return parentIsDampCaveSimple || parentIsRootJungle;
            }
            return true;
        }

        private void MultiplyChestCost(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            if (NetworkServer.active && IsValidScene(self.transform))
            {
                var chest = self.GetComponent<ChestBehavior>();

                if (chest && chest.tier3Chance == 1f)
                {
                    var ResultAmt = (int)Mathf.Ceil(self.cost * cfgCostMultiplier.Value);
                    self.Networkcost = ResultAmt;
                }
            }
            orig(self);
        }

        //override because i dunno IL
        private void DuplicateDrops(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (self.tier3Chance != 1 || self.dropPickup == PickupIndex.none || self.dropPickup == PickupIndex.none || !IsValidScene(self.transform))
            {
                orig(self);
                return;
            }
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.ChestBehavior::ItemDrop()' called on client");
                return;
            }

            int participatingPlayerCount = Run.instance.participatingPlayerCount != 0 ? Run.instance.participatingPlayerCount : 1;
            float angle = 360f / participatingPlayerCount;
            var chestVelocity = Vector3.up * self.dropUpVelocityStrength + self.dropTransform.forward * self.dropForwardVelocityStrength;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            int i = 0;
            while (i < participatingPlayerCount)
            {
                PickupDropletController.CreatePickupDroplet(self.dropPickup, self.dropTransform.position + Vector3.up * 1.5f, chestVelocity);
                i++;
                chestVelocity = rotation * chestVelocity;
            }
            self.dropPickup = PickupIndex.none;
        }
    }
}