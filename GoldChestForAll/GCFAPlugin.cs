using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace GoldChestForAll
{
    [BepInPlugin("com.DestroyedClone.GoldChestForAll", "GoldChestForAll", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class GCFAPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += GiveToEveryone;
        }

        private void GiveToEveryone(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
        }
    }
}
