using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
//using R2API.Utils;

namespace ForcedStageVariation
{
    [BepInPlugin("com.DestroyedClone.ForcedStageVariation", "Forced Stage Variation", "1.0.0")]
    //[BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class FSVPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> RootJungleTreasureChests { get; set; }
        public static ConfigEntry<int> RootJungleTunnelLandmass { get; set; }
        public static ConfigEntry<int> RootJungleHeldRocks { get; set; }
        public static ConfigEntry<int> RootJungleUndergroundShortcut { get; set; }

        public void Awake()
        {
            RootJungleTreasureChests = Config.Bind("Sundred Grove", "Treasure Chest Location", 2, "-1 = Default" +
                "\n0 = Root Bridge Front Chest" +
                "\n1 = Mushroom Cave Chest" +
                "\n2 = Treehouse Hole" +
                "\n3 = Triangle Cave" +
                "\n4 = Downed Tree Roots");
            RootJungleTunnelLandmass = Config.Bind("Sundred Grove", "Tunnel Landmass", 0, "-1 = Default" +
                "\n0 = Enabled" +
                "\n1 = No Tunnel Landmass");
            RootJungleHeldRocks = Config.Bind("Sundred Grove", "Held Rocks", 0, "-1 = Default" +
                "\n0 = Held Rock" +
                "\n1 = Split Rock");
            RootJungleUndergroundShortcut = Config.Bind("Sundred Grove", "Underground Shortcut", 0, "-1 = Default" +
                "\n0 = Open" +
                "\n1 = Closed");

            On.RoR2.SceneDirector.Start += ModifyScene;
            On.RoR2.Run.Awake += Run_Awake;
            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            orig(self);
            self.gameObject.AddComponent<FSVNetworkSync>();
        }

        private void ModifyScene(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                FSVNetworkSync f = Run.instance.GetComponent<FSVNetworkSync>();
                if (!f)
                {
                    f = Run.instance.gameObject.AddComponent<FSVNetworkSync>();
                }

                f.rootJungleTreasureChests = RootJungleTreasureChests.Value;
                f.rootJungleTunnelLandmass = RootJungleTunnelLandmass.Value;
                f.rootJungleHeldRocks = RootJungleHeldRocks.Value;
                f.rootJungleUndergroundShortcut = RootJungleUndergroundShortcut.Value;
            }
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "rootjungle":
                    ModifyRootJungle();
                    break;
            }
        }


        private void ModifyRootJungle()
        {
            FSVNetworkSync f = Run.instance.GetComponent<FSVNetworkSync>();
            if (!f)
            {
                Debug.LogError("Forced Stage Variation: Could not find syncing component, aborting!");
                return;
            }

            Debug.Log("Forced Stage Variation: Syncing randomization to host!");
            var randoHolder = GameObject.Find("HOLDER: Randomization").transform;

            var chestHolder = randoHolder.Find("GROUP: Large Treasure Chests");
            foreach (Transform child in chestHolder)
            {
                child.gameObject.SetActive(false);
            }
            switch (f.rootJungleTreasureChests)
            {
                case -1:
                default:
                    break;
                case 0:
                    chestHolder.Find("CHOICE: Root Bridge Front Chest").gameObject.SetActive(true);
                    break;
                case 1:
                    chestHolder.Find("CHOICE: Mushroom Cave Chest").gameObject.SetActive(true);
                    break;
                case 2:
                    chestHolder.Find("CHOICE: Treehouse Hole").gameObject.SetActive(true);
                    break;
                case 3:
                    chestHolder.Find("CHOICE: Triangle Cave").gameObject.SetActive(true);
                    break;
                case 4:
                    chestHolder.Find("CHOICE: Downed Tree Roots").gameObject.SetActive(true);
                    break;
            }

            var landmassHolder = randoHolder.Find("GROUP: Tunnel Landmass");
            foreach (Transform child in landmassHolder)
            {
                child.gameObject.SetActive(false);
            }
            switch (f.rootJungleTunnelLandmass)
            {
                case -1:
                default:
                    break;
                case 0:
                    landmassHolder.Find("CHOICE: Tunnel Landmass").gameObject.SetActive(true);
                    break;
                case 1:
                    landmassHolder.Find("CHOICE: NO Tunnel Landmass").gameObject.SetActive(true);
                    break;
            }

            var rockHolder = randoHolder.Find("GROUP: Held Rocks");
            foreach (Transform child in rockHolder)
            {
                child.gameObject.SetActive(false);
            }
            switch (f.rootJungleHeldRocks)
            {
                case -1:
                default:
                    break;
                case 0:
                    rockHolder.Find("CHOICE: Held Rock").gameObject.SetActive(true);
                    break;
                case 1:
                    rockHolder.Find("CHOICE: Split Rock").gameObject.SetActive(true);
                    break;
            }

            var shortcutHolder = randoHolder.Find("GROUP: Underground Shortcut");
            foreach (Transform child in shortcutHolder)
            {
                child.gameObject.SetActive(false);
            }
            switch (f.rootJungleUndergroundShortcut)
            {
                case -1:
                default:
                    break;
                case 0:
                    shortcutHolder.Find("CHOICE: Open").gameObject.SetActive(true);
                    break;
                case 1:
                    shortcutHolder.Find("CHOICE: Closed").gameObject.SetActive(true);
                    break;
            }
        }
    }
}
