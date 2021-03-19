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
            On.RoR2.ChestBehavior.ItemDrop += DuplicateDrops;
        }

        private void DuplicateDrops(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            orig(self);
            if (self.tier3Chance >= 1)
                for (int i = 0; i < CharacterMaster.readOnlyInstancesList.Count - 1; i++)
                {
                    CharacterMaster.readOnlyInstancesList[i]
                }
        }
    }
}
