using BepInEx;
using UnityEngine;
using RoR2;
using RoR2.UI;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using System.Text;

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

        public static ConfigEntry<float> cfgFirstStackDuration;
        public static ConfigEntry<float> cfgSubsequentStackDuration;

        public static ConfigEntry<float> cfgFirstStackTransformationDuration;
        public static ConfigEntry<float> cfgSubsequentStackTransformationDuration;

        public static GameObject NotificationPrefab;


        public void Start()
        {
            NotificationPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/NotificationPanel2.prefab").WaitForCompletion();

            cfgFirstStackDuration = Config.Bind("Item Notification", "First Stack Duration", 6f, "");
            cfgSubsequentStackDuration = Config.Bind("Item Notification", "Subsequent Stack Duration", 2f, "");

            cfgFirstStackTransformationDuration = Config.Bind("Transformation Notification", "First Stack Duration", 6f, "");
            cfgSubsequentStackTransformationDuration = Config.Bind("Transformation Notification", "Subsequent Stack Duration", 2f, "");

            RemoveFlash = Config.Bind("General", "Removes Flash", true);
            if (RemoveFlash.Value) NotificationPrefab.transform.Find("CanvasGroup/Flash").GetComponent<AnimateUIAlpha>().timeMax = 0f;

            FadeDurationDefault = Config.Bind("General", "Default Duration of the Fade Out", 0.5f);
            NotificationPrefab.GetComponent<GenericNotification>().fadeOutT = FadeDurationDefault.Value;

            NotificationDurationDefault = Config.Bind("", "Initial Duration of Pickup Notification", 6f);

            ClearTopNotification = Config.Bind("General", "Immediately replaces the topmost pickup notification when picking up", false);

            On.RoR2.CharacterMasterNotificationQueue.PushNotification += CharacterMasterNotificationQueue_PushNotification;
            On.RoR2.UI.NotificationUIController.SetUpNotification += NotificationUIController_SetUpNotification;
            //On.RoR2.CharacterMasterNotificationQueue.NotificationInfo.
        }

        private void NotificationUIController_SetUpNotification(On.RoR2.UI.NotificationUIController.orig_SetUpNotification orig, NotificationUIController self, CharacterMasterNotificationQueue.NotificationInfo notificationInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendLine("setupnotif");
            if (notificationInfo.transformation != null)
            {
                sb.AppendLine("Transformation");
                if (notificationInfo.data is ItemDef def && def)
                {
                    sb.AppendLine($"Item Count: {self.targetMaster.inventory.GetItemCount(def)}");
                    notificationInfo.data = self.targetMaster.inventory.GetItemCount(def) <= 1 ? cfgFirstStackTransformationDuration.Value : cfgSubsequentStackTransformationDuration.Value;
                }
                else if (notificationInfo.data is EquipmentDef equipmentDef && equipmentDef)
                {
                    sb.AppendLine($"isEquipment");
                    self.notificationQueue.notifications[0].duration = cfgFirstStackTransformationDuration.Value;
                }
            }
            else
            {
                sb.AppendLine("Normal");
                if (notificationInfo.data is ItemDef def && def)
                {
                    sb.AppendLine($"Item Count: {self.targetMaster.inventory.GetItemCount(def)}");
                    self.notificationQueue.notifications[0].duration = self.targetMaster.inventory.GetItemCount(def) <= 1 ? cfgFirstStackDuration.Value : cfgSubsequentStackDuration.Value;
                }
                else
                {
                    sb.AppendLine($"isEquipment");
                    self.notificationQueue.notifications[0].duration = cfgFirstStackDuration.Value;
                }
            }
            if (ClearTopNotification.Value && self.notificationQueue.notifications.Count > 0)
            {
                self.notificationQueue.notifications[0].duration = 0f;
            }
            Debug.Log(sb.ToString());
            orig(self, notificationInfo);
        }

        private void CharacterMasterNotificationQueue_PushNotification(On.RoR2.CharacterMasterNotificationQueue.orig_PushNotification orig, CharacterMasterNotificationQueue self, CharacterMasterNotificationQueue.NotificationInfo info, float duration)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            if (info.transformation != null)
            {
                sb.AppendLine("Transformation");
                if (info.data is ItemDef def && def)
                {
                    sb.AppendLine($"Item Count: {self.master.inventory.GetItemCount(def)}");
                    duration = self.master.inventory.GetItemCount(def) <= 1 ? cfgFirstStackTransformationDuration.Value : cfgSubsequentStackTransformationDuration.Value;
                }
                else if (info.data is EquipmentDef equipmentDef && equipmentDef)
                {
                    sb.AppendLine($"isEquipment");
                    duration = cfgFirstStackTransformationDuration.Value;
                }
            } else
            {
                sb.AppendLine("Normal");
                if (info.data is ItemDef def && def)
                {
                    sb.AppendLine($"Item Count: {self.master.inventory.GetItemCount(def)}");
                    duration = self.master.inventory.GetItemCount(def) <= 1 ? cfgFirstStackDuration.Value : cfgSubsequentStackDuration.Value;
                }
                else
                {
                    sb.AppendLine($"isEquipment");
                    duration = cfgFirstStackDuration.Value;
                }
            }
            if (ClearTopNotification.Value && self.notifications.Count > 0)
            {
                self.notifications[0].duration = 0f;
            }
            Debug.Log(sb.ToString());
            orig(self, info, duration);
        }

        /*public void Start()
        {

            Notification = LegacyResourcesAPI.Load<GameObject>("prefabs/NotificationPanel2");
            RemoveFlash = Config.Bind("", "Removes Flash", true);
            NotificationDurationDefault = Config.Bind("", "Initial Duration of Pickup Notification", 6f);
            FadeDurationDefault = Config.Bind("", "Default Duration of the Fade Out", 0.5f);
            ClearTopNotification = Config.Bind("", "Immediately replaces the topmost pickup notification when picking up", true);

            if (RemoveFlash.Value) Notification.transform.Find("Flash").GetComponent<AnimateUIAlpha>().timeMax = 0f;

            Notification.GetComponent<GenericNotification>().fadeOutT = NotificationDurationDefault.Value;

            if (ClearTopNotification.Value)
                On.RoR2.CharacterMasterNotificationQueue.PushNotification += ClearTopNotifMethod;
        }

        private void ClearTopNotifMethod(On.RoR2.CharacterMasterNotificationQueue.orig_PushNotification orig, CharacterMasterNotificationQueue self, CharacterMasterNotificationQueue.NotificationInfo info, float duration)
        {
            orig(self, info, duration);

            CharacterMasterNotificationQueue.TimedNotificationInfo timedNotificationInfo = self.notifications[0];
            timedNotificationInfo.duration = 0;
        }*/
    }
}
