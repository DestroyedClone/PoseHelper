using BepInEx;
using UnityEngine;
using RoR2;
using RoR2.UI;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using BepInEx.Configuration;

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
        public static ConfigEntry<bool> RemoveFlash { get; set; }
        public static ConfigEntry<float> NotificationDurationDefault { get; set; }
        public static ConfigEntry<float> FadeDurationDefault { get; set; }
        public static ConfigEntry<bool> ClearTopNotification { get; set; }

        public GameObject Notification = Resources.Load<GameObject>("prefabs/NotificationPanel2");

        public void Awake()
        {
            RemoveFlash = Config.Bind("", "Removes Flash", true);
            NotificationDurationDefault = Config.Bind("", "Initial Duration of Pickup Notification", 6f);
            FadeDurationDefault = Config.Bind("", "Default Duration of the Fade Out", 0.5f);
            ClearTopNotification = Config.Bind("", "Immediately replaces the topmost pickup notification when picking up", true);

            if (RemoveFlash.Value) Notification.transform.Find("Flash").GetComponent<AnimateUIAlpha>().timeMax = 0f;

            Notification.GetComponent<GenericNotification>().duration = NotificationDurationDefault.Value;

            if (ClearTopNotification.Value)
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
