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
using UnityEngine.Events;

using System.Collections.Generic;
[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AdditionalDeployables
{
    [BepInPlugin("com.DestroyedClone.AdditionalDeployables", "Additional Deployables", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(DeployableAPI), nameof(PrefabAPI))]
    public class MainDeployable : BaseUnityPlugin
    {
        // only fucking reference for deployables:
        // https://github.com/TheMysticSword/EliteVariety/blob/20cdab13a41dd86e8f1108ba30696ae2b1ca9efe/Buffs/AffixTinkerer.cs
        // https://github.com/Theray070696/ConfigurableDollLimit/blob/master/Configurable%20Doll%20Limit/ConfigurableDollLimit.cs
        public static GameObject scanner = Resources.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner");
        public static GameObject gateway = Resources.Load<GameObject>("Prefabs/NetworkedObjects/Zipline");
        public static GameObject saw = Resources.Load<GameObject>("Prefabs/Projectiles/Sawmerang");
        public static GameObject blackhole = Resources.Load<GameObject>("Prefabs/Projectiles/GravSphere");

        public static DeployableSlot deployableSlot_Scanner;
        public static DeployableSlot deployableSlot_Gateway;
        public static DeployableSlot deployableSlot_Saw;
        public static DeployableSlot deployableSlot_Blackhole;

        public static UnityEvent undeployScanner;

        public static int cfgMaxScanner = 1;
        public static int cfgMaxGateway = 5;
        public static int cfgMaxSaw = 3;
        public static int cfgMaxBlackhole = 2;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            SetupDeployableSlots();
            ModifyPrefabs();
            if (!scanner.GetComponent<Deployable>())
            {
                _logger.LogError("fuck");
            }

            On.RoR2.EquipmentSlot.FireScanner += EquipmentSlot_FireScanner;
            On.RoR2.EquipmentSlot.FireGateway += EquipmentSlot_FireGateway;
            On.RoR2.EquipmentSlot.FireSaw += EquipmentSlot_FireSaw;
            On.RoR2.EquipmentSlot.FireBlackhole += EquipmentSlot_FireBlackhole;

            _logger.LogMessage($"Modded deployable count: {DeployableAPI.ModdedDeployableSlotCount}");
        }

        #region Overrides
        private bool CanDeploy(EquipmentSlot equipmentSlot, DeployableSlot deployableSlot)
        {
            CharacterMaster master = equipmentSlot.characterBody.master;
            if (!master)
            {
                return false;
            }
            if (master.GetDeployableCount(deployableSlot) >= master.GetDeployableSameSlotLimit(deployableSlot))
            {
                return false;
            }
            return true;
        }

        private bool EquipmentSlot_FireScanner(On.RoR2.EquipmentSlot.orig_FireScanner orig, EquipmentSlot self)
        {
            if (CanDeploy(self, deployableSlot_Scanner))
            {
                return orig(self);
            }
            return false;
        }

        private bool EquipmentSlot_FireGateway(On.RoR2.EquipmentSlot.orig_FireGateway orig, EquipmentSlot self)
        {
            if (CanDeploy(self, deployableSlot_Gateway))
            {
                return orig(self);
            }
            return false;
        }

        private bool EquipmentSlot_FireSaw(On.RoR2.EquipmentSlot.orig_FireSaw orig, EquipmentSlot self)
        {
            if (CanDeploy(self, deployableSlot_Saw))
            {
                return orig(self);
            }
            return false;
        }

        private bool EquipmentSlot_FireBlackhole(On.RoR2.EquipmentSlot.orig_FireBlackhole orig, EquipmentSlot self)
        {
            if (CanDeploy(self, deployableSlot_Blackhole))
            {
                return orig(self);
            }
            return false;
        }
        #endregion


        public static void ModifyPrefabs()
        {
            if (scanner)
            {
                scanner.AddComponent<Deployable>().onUndeploy = undeployScanner;
                scanner.AddComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = deployableSlot_Scanner;
            }
            if (gateway)
            {
                gateway.AddComponent<Deployable>();
                gateway.AddComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = deployableSlot_Gateway;
            }
            if (saw)
            {
                saw.AddComponent<Deployable>();
                saw.AddComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = deployableSlot_Saw;
            }
            if (blackhole)
            {
                blackhole.AddComponent<Deployable>();
                blackhole.AddComponent<RoR2.Projectile.ProjectileDeployToOwner>().deployableSlot = deployableSlot_Blackhole;
            }
        }

        public static void SetupDeployableSlots()
        {
            deployableSlot_Scanner = DeployableAPI.RegisterDeployableSlot(GetScannerDeployableSameSlotLimit);
            deployableSlot_Gateway = DeployableAPI.RegisterDeployableSlot(GetGatewayDeployableSameSlotLimit);
            deployableSlot_Saw = DeployableAPI.RegisterDeployableSlot(GetSawDeployableSameSlotLimit);
            deployableSlot_Blackhole = DeployableAPI.RegisterDeployableSlot(GetBlackholeDeployableSameSlotLimit);
        }

        public static int GetScannerDeployableSameSlotLimit(CharacterMaster self, int deployableCountMultiplier)
        {
            return cfgMaxScanner;
        }

        public static int GetGatewayDeployableSameSlotLimit(CharacterMaster self, int deployableCountMultiplier)
        {
            return cfgMaxGateway;
        }

        public static int GetSawDeployableSameSlotLimit(CharacterMaster self, int deployableCountMultiplier)
        {
            return cfgMaxSaw;
        }

        public static int GetBlackholeDeployableSameSlotLimit(CharacterMaster self, int deployableCountMultiplier)
        {
            return cfgMaxBlackhole;
        }
    }
}
