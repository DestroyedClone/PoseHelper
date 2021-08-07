using BepInEx;
using UnityEngine;
using RoR2;
using RoR2.UI;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace FasterPickupText
{
    [BepInPlugin("com.DestroyedClone.FasterPickupText", "Faster Pickup Text", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class FasterPickupTextPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.NotificationQueue.OnPickup += NotificationQueue_OnPickup;
        }

        private void NotificationQueue_OnPickup(On.RoR2.UI.NotificationQueue.orig_OnPickup orig, RoR2.UI.NotificationQueue self, CharacterMaster characterMaster, PickupIndex pickupIndex)
        {
            orig(self, characterMaster, pickupIndex);
            if (self.currentNotification)
                self.currentNotification.duration = 0;
        }
    }
}
