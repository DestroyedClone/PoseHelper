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
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
//dotnet build --configuration Release

namespace FasterPickupText
{
    [BepInPlugin("com.DestroyedClone.FasterPickupText", "Faster Pickup Text", "1.1.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class FasterPickupTextPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> RemoveFlash { get; set; }
        //public static ConfigEntry<float> NotificationDurationDefault { get; set; }
        public static ConfigEntry<float> FadeDurationDefault { get; set; }
        public static ConfigEntry<bool> ClearTopNotification { get; set; }

        public static ConfigEntry<float> cfgNewItemOverride;
        public static ConfigEntry<float> cfgFirstStackDuration;
        public static ConfigEntry<float> cfgSubsequentStackDuration;
        public static ConfigEntry<float> cfgNewEquipmentOverride;
        public static ConfigEntry<float> cfgEquipmentDuration;

        public static ConfigEntry<float> cfgFirstStackTransformationDuration;
        public static ConfigEntry<float> cfgSubsequentStackTransformationDuration;

        public static GameObject NotificationPrefab;


        public void Start()
        {
            NotificationPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/NotificationPanel2.prefab").WaitForCompletion();

            RemoveFlash = Config.Bind("Notifications", "Remove Flash", true, "If true, then the notification popup will not have an initial flash.");
            if (RemoveFlash.Value) NotificationPrefab.transform.Find("CanvasGroup/Flash").GetComponent<AnimateUIAlpha>().timeMax = 0f;

            FadeDurationDefault = Config.Bind("Notifications", "Fade Out Duration", 0.5f, "The duration of time, in seconds, of the fade-out time of notifications.");
            NotificationPrefab.GetComponent<GenericNotification>().fadeOutT = FadeDurationDefault.Value;

            cfgNewItemOverride = Config.Bind("Item Notification", "NEW Item Duration", 6f, "Overrides first stack/subsequent stack durations.\nThe duration of time, in seconds, of how long to show the pickup notification of an item that hasn't been picked up before for the profile.");
            cfgFirstStackDuration = Config.Bind("Item Notification", "First Stack Duration", 4f, "The duration of time, in seconds, of how long to show the item pickup notification for the first stack of item.");
            cfgSubsequentStackDuration = Config.Bind("Item Notification", "Subsequent Stack Duration", 2f, "The duration of time, in seconds, of how long to show the item pickup notification for the subsequent stack of an item.");

            cfgNewEquipmentOverride = Config.Bind("Equipment Notification", "NEW Equipment Duration", 6f, "Overrides equipment pickup duration.\nThe duration of time, in seconds, of how long to show the pickup notification of an item that hasn't been picked up before for the profile.");
            cfgEquipmentDuration = Config.Bind("Equipment Notification", "Equipment Pickup Duration", 4f, "The duration of time, in seconds, of how long to show the equipment pickup notification.");


            cfgFirstStackTransformationDuration = Config.Bind("Transformation Notification", "First Stack Duration", 6f, "The duration of time, in seconds, of how long to show the transformation notification for the first stack of an item.");
            cfgSubsequentStackTransformationDuration = Config.Bind("Transformation Notification", "Subsequent Stack Duration", 3f, "The duration of time, in seconds, of how long to show the initial transformation notification duration for the subsequent stack of an item.");


            //old
            //NotificationDurationDefault = Config.Bind("", "Initial Duration of Pickup Notification", 6f);

            ClearTopNotification = Config.Bind("Notifications", "Immediately replaces the topmost pickup notification when picking up", false);

            On.RoR2.CharacterMasterNotificationQueue.PushNotification += CharacterMasterNotificationQueue_PushNotification;
            //On.RoR2.UI.NotificationUIController.SetUpNotification += NotificationUIController_SetUpNotification;
            //On.RoR2.CharacterMasterNotificationQueue.NotificationInfo.
            //On.RoR2.UserProfile.DiscoverPickup += QueueDiscoveredPickup;
            RoR2.Run.onRunStartGlobal += ClearDiscoveredPickups;
            RoR2.LocalUserManager.onUserSignIn += SubscribeToProfile;
            RoR2.LocalUserManager.onUserSignOut += ClearProfileSubscription;
        }

        public void SubscribeToProfile(LocalUser localUser)
        {
            localUser.userProfile.onPickupDiscovered += QueueDiscoveredPickup;
        }
        public void ClearProfileSubscription(LocalUser localUser)
        {
            localUser.userProfile.onPickupDiscovered -= QueueDiscoveredPickup;
        }

        public void ClearDiscoveredPickups(Run run)
        {
            queuedItemDefs.Clear();
            queuedEquipmentDefs.Clear();
            queuedArtifactDefs.Clear();
            queuedMiscPickupDefs.Clear();
        }


        //better to queue it in advance? is this more performant?
        // update: what does this mean?
        public static List<ItemDef> queuedItemDefs = new List<ItemDef>();
        public static List<EquipmentDef> queuedEquipmentDefs = new List<EquipmentDef>();
        public static List<ArtifactDef> queuedArtifactDefs = new List<ArtifactDef>();
        public static List<MiscPickupDef> queuedMiscPickupDefs = new List<MiscPickupDef>();

        //On.RoR2.UserProfile.orig_DiscoverPickup orig, 
        private void QueueDiscoveredPickup(PickupIndex pickupIndex)
        {
            //orig(self, pickupIndex);
            var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            if (pickupDef != null)
            {
                if (pickupDef.itemIndex != ItemIndex.None)
                {
                    queuedItemDefs.Add(ItemCatalog.GetItemDef(pickupDef.itemIndex));
                }
                else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    queuedEquipmentDefs.Add(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex));
                }
                else if (pickupDef.artifactIndex != ArtifactIndex.None)
                {
                    queuedArtifactDefs.Add(ArtifactCatalog.GetArtifactDef(pickupDef.artifactIndex));
                }
                else
                {
                    //wtf does arrayutils come from?
                    //var miscPickupDef = ArrayUtils.GetSafe<MiscPickupDef>(MiscPickupCatalog._miscPickupDefs, (int)itemIndex);
                    //queuedMiscPickupDefs.Add(miscPickupDef);
                }
            }
        }

#region cum
/*
        private void NotificationUIController_SetUpNotification(On.RoR2.UI.NotificationUIController.orig_SetUpNotification orig, NotificationUIController self, CharacterMasterNotificationQueue.NotificationInfo notificationInfo)
        {
            orig(self, notificationInfo);
            float newDuration = self.data.duration;
            bool isTransformation = notificationInfo.transformation != null;
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendLine("NotificationUIController_SetUpNotification");

            if (notificationInfo.data is RoR2.ItemDef itemDef)
            { 
                sb.AppendLine($"Item: {Language.GetString(itemDef.nameToken)}");
                //New Item Check
                if (queuedItemDefs.Contains(itemDef))
                {
                    queuedItemDefs.Remove(itemDef);
                    sb.AppendLine("New Item!");
                    newDuration = cfgNewItemOverride.Value;
                }
                else {
                    var itemCount = self.targetMaster.inventory.GetItemCount(itemDef);
                    sb.AppendLine($"Count x{itemCount}");
                    if (isTransformation)
                    {
                        sb.Append(" (Transformation)");
                        newDuration = itemCount <= 1 ? cfgFirstStackTransformationDuration.Value : cfgSubsequentStackTransformationDuration.Value;
                    } else {
                        newDuration = itemCount <= 1 ? cfgFirstStackDuration.Value : cfgSubsequentStackDuration.Value;
                    }
                }
            } 
            else if (notificationInfo.data is EquipmentDef equipmentDef)
            {
                sb.AppendLine($"Equipment: {Language.GetString(equipmentDef.nameToken)}");
                //New Equipment Check
                if (queuedEquipmmentDefs.Contains(equipmentDef))
                {
                    queuedEquipmentDefs.Remove(equipmentDef);
                    sb.AppendLine($"New Equipment!");
                    newDuration = cfgNewEquipmentOverride.Value;
                } else {
                    newDuration = cfgEquipmentDuration.Value;
                }
            }
            else if (notificationInfo.data is ArtifactDef artifactDef)
            {
                sb.AppendLine("ARTIFACT");
            }
            else {
                sb.AppendLine("OTHER PICKUP");
            }
            //self.notificationQueue.notifications[0].duration = cfgFirstStackTransformationDuration.Value;
            // == Final Object = newDuration == //

            if (ClearTopNotification.Value && self.notificationQueue.notifications.Count > 0)
            {
                self.notificationQueue.notifications[0].duration = 0f;
            }
            Debug.Log(sb.ToString());
        }
*/
#endregion
        private void CharacterMasterNotificationQueue_PushNotification(On.RoR2.CharacterMasterNotificationQueue.orig_PushNotification orig, CharacterMasterNotificationQueue self, CharacterMasterNotificationQueue.NotificationInfo notificationInfo, float duration)
        {
            float newDuration = duration;
            bool isTransformation = notificationInfo.transformation != null;
            //StringBuilder sb = new StringBuilder();
            //sb.Clear();

            if (notificationInfo.data is RoR2.ItemDef itemDef)
            { 
                ///sb.AppendLine($"Item: {Language.GetString(itemDef.nameToken)}");
                //New Item Check
                if (queuedItemDefs.Contains(itemDef))
                {
                    queuedItemDefs.Remove(itemDef);
                    //sb.AppendLine("New Item!");
                    newDuration = cfgNewItemOverride.Value;
                }
                else {
                    var itemCount = self.master.inventory.GetItemCount(itemDef);
                    //sb.AppendLine($"Count x{itemCount}");
                    if (isTransformation)
                    {
                        //sb.Append(" (Transformation)");
                        newDuration = itemCount <= 1 ? cfgFirstStackTransformationDuration.Value : cfgSubsequentStackTransformationDuration.Value;
                    } else {
                        newDuration = itemCount <= 1 ? cfgFirstStackDuration.Value : cfgSubsequentStackDuration.Value;
                    }
                }
            } 
            else if (notificationInfo.data is EquipmentDef equipmentDef)
            {
                ///sb.AppendLine($"Equipment: {Language.GetString(equipmentDef.nameToken)}");
                //New Equipment Check
                if (queuedEquipmentDefs.Contains(equipmentDef))
                {
                    queuedEquipmentDefs.Remove(equipmentDef);
                    //sb.AppendLine($"New Equipment!");
                    newDuration = cfgNewEquipmentOverride.Value;
                } else {
                    newDuration = cfgEquipmentDuration.Value;
                }
            }
            else if (notificationInfo.data is ArtifactDef artifactDef)
            {
                //sb.AppendLine("ARTIFACT");
            }
            else {
                //sb.AppendLine("OTHER PICKUP");
            }
            //self.notificationQueue.notifications[0].duration = cfgFirstStackTransformationDuration.Value;
            // == Final Object = newDuration == //

            if (ClearTopNotification.Value && self.notifications.Count > 0)
            {
                self.notifications[0].duration = 0f;
            }

            duration = newDuration;
            //sb.AppendLine($"New Duration: {newDuration}");
            //Debug.Log(sb.ToString());

            orig(self, notificationInfo, duration);
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