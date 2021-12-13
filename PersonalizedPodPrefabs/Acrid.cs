using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Acrid : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Croco.bodyPrefab.name;
        public override bool ShouldAddVolatileBatteryHook => true;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, PodPrefabName);
            podPrefab.AddComponent<AcridPodComponent>();
            return podPrefab;
        }

        public class AcridPodComponent : PodComponent
        {
            private readonly int acidPoolAmount = 8;
            private readonly float acidPoolDistance = 8f;

            protected override void Start()
            {
                addExitAction = true;
                addLandingAction = false;
                base.Start();
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    SpawnAcidPools(characterBody);
                    if (cfgShouldDropVolatileBattery)
                        PersonalizePodPlugin.SpawnBattery(characterBody.footPosition);
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
                float angle = 360f / acidPoolAmount;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                var nextPosition = passengerBody.footPosition + Vector3.forward * acidPoolDistance;

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