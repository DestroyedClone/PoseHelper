using BepInEx;
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

        public override void Init(ConfigFile config)
        {
            SetupPod();
        }

        private void SetupPod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, "AcridPodPrefab");
            RoR2Content.Survivors.Croco.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = podPrefab;
            podPrefab.AddComponent<AcridPodComponent>();
        }

        public class AcridPodComponent : PodComponent
        {
            protected override void VehicleSeat_onPassengerExit(GameObject obj)
            {
                Vector3 position = obj.transform.position;
                var characterBody = obj.GetComponent<CharacterBody>();
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
                    owner = obj,
                    rotation = Quaternion.identity,
                    position = position
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
    }
}
