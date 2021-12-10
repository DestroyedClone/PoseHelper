﻿using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using RoR2.Projectile;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Acrid : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Croco.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, PodPrefabName);
            podPrefab.AddComponent<AcridPodComponent>();
            return podPrefab;
        }

        public class AcridPodComponent : PodComponent
        {
            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    SpawnAcidPools(characterBody);
                }
            }

            private void SpawnAcidPool(CharacterBody characterBody, Vector3 position)
            {
                if (characterBody)
                {
                    position = characterBody.footPosition;
                }
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = EntityStates.Croco.BaseLeap.projectilePrefab,
                    crit = false,
                    force = 0f,
                    damage = characterBody ? characterBody.damage : 18f,
                    owner = characterBody.gameObject,
                    rotation = Quaternion.identity,
                    position = position
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            private void SpawnAcidPools(CharacterBody passengerBody)
            {
                int acidPoolAmount = 8;
                float angle = 360f / acidPoolAmount;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                var nextPosition = passengerBody.footPosition + Vector3.forward * 6f;

                int i = 0;
                while (i < acidPoolAmount)
                { // I *would* prefer raytraces
                    SpawnAcidPool(passengerBody, nextPosition);
                    i++;
                    nextPosition = rotation * nextPosition;
                }
            }
        }
    }
}
