using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using static ProjectileLimiter.Main;
//using static AdditionalDeployables.ServerTrackers;

namespace ProjectileLimiter
{
    public class Overrides
    {
        public static void RunOverrides()
        {
            On.RoR2.EquipmentSlot.FireScanner += EquipmentSlot_FireScanner;
            On.RoR2.EquipmentSlot.FireGateway += EquipmentSlot_FireGateway;
            On.RoR2.EquipmentSlot.FireSaw += EquipmentSlot_FireSaw;
            On.RoR2.EquipmentSlot.FireBlackhole += EquipmentSlot_FireBlackhole;
            On.RoR2.EquipmentSlot.FireMeteor += EquipmentSlot_FireMeteor;
        }

        #region Overrides

        private static bool EquipmentSlot_FireMeteor(On.RoR2.EquipmentSlot.orig_FireMeteor orig, EquipmentSlot self)
        {
            if (CanDeploy(self, PerPlayerDeployableType.Meteorite))
            {
                self.subcooldownTimer = cfgMeteoriteCooldown;
                return orig(self);
            }
            return false;
        }

        private static bool CanDeploy(EquipmentSlot equipmentSlot, PerPlayerDeployableType deployableType)
        {
            ProjectileDeployableTracker tracker = equipmentSlot.characterBody.GetComponent<ProjectileDeployableTracker>();
            if (!tracker)
            {
                return false;
            }
            return tracker.CheckSummonAvailability(deployableType);
        }

        private static bool EquipmentSlot_FireScanner(On.RoR2.EquipmentSlot.orig_FireScanner orig, EquipmentSlot self)
        {
            var original = orig(self);
            if (original) self.subcooldownTimer = cfgScannerCooldown;
            return original;
        }

        private static bool EquipmentSlot_FireGateway(On.RoR2.EquipmentSlot.orig_FireGateway orig, EquipmentSlot self)
        {
            var original = orig(self);
            if (original) self.subcooldownTimer = cfgGatewayCooldown;
            return original;
        }

        private static bool EquipmentSlot_FireSaw(On.RoR2.EquipmentSlot.orig_FireSaw orig, EquipmentSlot self)
        {
            if (CanDeploy(self, PerPlayerDeployableType.Saw))
            {
                self.subcooldownTimer = cfgSawCooldown;
                return orig(self);
            }
            return false;
        }

        private static bool EquipmentSlot_FireBlackhole(On.RoR2.EquipmentSlot.orig_FireBlackhole orig, EquipmentSlot self)
        {
            var original = orig(self);
            if (original)
                self.subcooldownTimer = cfgBlackholeCooldown;
            return original;
        }

        #endregion Overrides
    }
}