using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Collections.Generic;
using EntityStates.AI;
using System.Linq;
using UnityEngine.Networking;

namespace UniqueItemsArtifact
{
    public class InstanceTracking
    {
        public class InstanceTrackerRemover : MonoBehaviour
        {
            public ChestBehavior chestBehavior;

            public void OnDestroy()
            {
                InstanceTracker.Remove(chestBehavior);
            }
        }

        public static void Track_ChestBehavior(On.RoR2.ChestBehavior.orig_Awake orig, ChestBehavior self)
        {
            orig(self);
            InstanceTracker.Add(self);
            if (!self.GetComponent<InstanceTrackerRemover>())
                self.gameObject.AddComponent<InstanceTrackerRemover>().chestBehavior = self;
        }

        public static void Track_MultiShopCrontroller_Remove(On.RoR2.MultiShopController.orig_OnDestroy orig, MultiShopController self)
        {
            orig(self);
            InstanceTracker.Remove(self);
        }

        public static void Track_MultiShopCrontroller_Add(On.RoR2.MultiShopController.orig_Awake orig, MultiShopController self)
        {
            orig(self);
            InstanceTracker.Add(self);
        }
    }
}
