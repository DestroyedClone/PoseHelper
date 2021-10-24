using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using BetterUI;
using static BetterUI.StatsDisplay;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AccuracyTest
{
    [BepInPlugin("com.DestroyedClone.AccuracyTest", "Accuracy Test", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.HardDependency)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI), nameof(NetworkingAPI))]
    public class Main : BaseUnityPlugin
    {

        public void Start()
        {
            On.RoR2.GlobalEventManager.OnHitAll += GlobalEventManager_OnHitAll;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            BetterUI.StatsDisplay.AddStatsDisplay("$accuracy", (BetterUI.StatsDisplay.DisplayCallback)GetAccuracy);
            BetterUI.StatsDisplay.AddStatsDisplay("$accuracybind", (BetterUI.StatsDisplay.DisplayCallback)GetAccuracyBind);
        }

        private static string GetAccuracy(CharacterBody body)
        {
            string value = null;
            var accuracyTracker = body.GetComponent<AccuracyTracker>();
            if (accuracyTracker)
            {
                return $"{accuracyTracker.accuracy*100f}%";
            }
            return value;
        }

        private static string GetAccuracyBind(CharacterBody body)
        {
            string value = null;
            var accuracyTracker = body.GetComponent<AccuracyTracker>();
            if (accuracyTracker)
            {
                return $"({accuracyTracker.hitEnemyCount} / {accuracyTracker.hitAllCount})" +
                    $"\nMisses: {accuracyTracker.hitAllCount - accuracyTracker.hitEnemyCount}";
            }
            return value;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            obj.gameObject.AddComponent<AccuracyTracker>();
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker)
            {
                var com = damageInfo.attacker.GetComponent<AccuracyTracker>();
                if (com)
                {
                    com.hitEnemyCount++;
                }
            }
        }

        private void GlobalEventManager_OnHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            if (damageInfo.attacker)
            {
                var com = damageInfo.attacker.GetComponent<AccuracyTracker>();
                if (com)
                {
                    com.hitAllCount++;
                }
            }
        }

        public class AccuracyTracker : MonoBehaviour
        {
            public uint hitAllCount = 1U;
            public uint hitEnemyCount = 1U;
            public float accuracy = 1;

            void FixedUpdate()
            {
                accuracy = (float)hitEnemyCount / (float)hitAllCount;
            }
        }
    }
}
