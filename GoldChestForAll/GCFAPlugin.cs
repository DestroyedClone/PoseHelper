using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System;
using System.Collections.Generic;
using EntityStates;
using EntityStates.Barrel;
using RoR2.Networking;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GoldChestForAll
{
    [BepInPlugin("com.DestroyedClone.GoldChestForAll", "GoldChestForAll", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class GCFAPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.ChestBehavior.ItemDrop += DuplicateDrops;
        }

        //override because i dunno IL
        private void DuplicateDrops(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.ChestBehavior::ItemDrop()' called on client");
                return;
            }
            if (self.dropPickup == PickupIndex.none)
            {
                return;
            }

			int participatingPlayerCount = Run.instance.participatingPlayerCount;
            if (participatingPlayerCount != 0)
            {
                float angle = 360f / (float)participatingPlayerCount;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                int i = 0;
                while (i < participatingPlayerCount)
                {
                    PickupDropletController.CreatePickupDroplet(self.dropPickup, self.dropTransform.position + Vector3.up * 1.5f, vector);
                    i++;
                    vector = rotation * vector;
                }


                //var velocity = Vector3.up * self.dropUpVelocityStrength + self.dropTransform.forward * self.dropForwardVelocityStrength;
                //PickupDropletController.CreatePickupDroplet(self.dropPickup, self.dropTransform.position + Vector3.up * 1.5f, velocity);
                self.dropPickup = PickupIndex.none;
            }
        }
    }
}
