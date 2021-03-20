using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace ForcedStageVariation
{
    [BepInPlugin("com.DestroyedClone.ForcedStageVariation", "Forced Stage Variation", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class FSVPlugin
    {
        public static ConfigEntry<bool> rootJungleTreasureChests { get; set; }
        public static ConfigEntry<bool> rootJungleTunnelLandmass { get; set; }
        public static ConfigEntry<bool> rootJungleHeldRocks { get; set; }
        public static ConfigEntry<bool> rootJungleUndergroundShortcut { get; set; }

        public void Awake()
        {

        }
    }
}
