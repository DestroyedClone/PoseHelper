using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using UnityEngine.Networking;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace GoldChestForAll
{
    [BepInPlugin("com.DestroyedClone.GoldChestForAll", "GoldChestForAll", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class GCFAPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> CostMultiplier { get; set; }

        public void Awake()
        {
            CostMultiplier = Config.Bind("Default", "Gold Chest Cost Multiplier", 1.25f, "Multiply the costs of gold chests. Intended for balance, but you can just set it to '1' if you want it unchanged.");

            On.RoR2.ChestBehavior.ItemDrop += DuplicateDrops;

            if (CostMultiplier.Value != 1f)
            {
                On.RoR2.PurchaseInteraction.Awake += MultiplyChestCost;
            }
        }

        private void MultiplyChestCost(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            var chest = self.GetComponent<ChestBehavior>();

            if (chest && chest.tier3Chance == 1f)
            {
                var ResultAmt = (int)Mathf.Ceil(self.cost * CostMultiplier.Value);
                self.cost = ResultAmt;
                self.Networkcost = ResultAmt;
            }
            orig(self);
        }

        //override because i dunno IL
        private void DuplicateDrops(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (self.tier3Chance != 1)
            {
                orig(self);
                return;
            }
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.ChestBehavior::ItemDrop()' called on client");
                return;
            }
            if (self.dropPickup == PickupIndex.none)
            {
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
