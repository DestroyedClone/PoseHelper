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
    public class FSVPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> rootJungleTreasureChests { get; set; }
        public static ConfigEntry<bool> rootJungleTunnelLandmass { get; set; }
        public static ConfigEntry<bool> rootJungleHeldRocks { get; set; }
        public static ConfigEntry<bool> rootJungleUndergroundShortcut { get; set; }

        public void Awake()
        {
            rootJungleTreasureChests = Config.Bind("Sundred Grove", "Treasure Chest Location", true, "-1 = Default" +
                "\n0 = Root Bridge Front Chest" +
                "\n1 = Mushroom Cave Chest" +
                "\n2 = Treehouse Hole" +
                "\n3 = Triangle Cave" +
                "\n4 = Downed Tree Roots");
            rootJungleTunnelLandmass = Config.Bind("Sundred Grove", "Tunnel Landmass", true, "-1 = Default" +
                "\n0 = Enabled" +
                "\n1 = No Tunnel Landmass");
            rootJungleHeldRocks = Config.Bind("Sundred Grove", "Held Rocks", true, "-1 = Default" +
                "\n0 = Held Rock" +
                "\n1 = Split Rock");
            rootJungleUndergroundShortcut = Config.Bind("Sundred Grove", "Underground Shortcut", true, "-1 = Default" +
                "\n0 = Open" +
                "\n1 = Closed");


        }


    }
}
