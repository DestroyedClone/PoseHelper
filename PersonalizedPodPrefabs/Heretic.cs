using R2API;
using RoR2;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Heretic : PodBase
    {
        public override string BodyName => "HereticBody";
        public override bool ShouldAddVolatileBatteryHook => true;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, PodPrefabName);
            podPrefab.AddComponent<HereticPodComponent>();
            return podPrefab;
        }

        public class HereticPodComponent : PodComponent
        {
            protected override void Start()
            {
                addLandingAction = false;
                addExitAction = true;
                base.Start();
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody && isServer)
                {
                    characterBody.AddTimedBuff(RoR2Content.Buffs.LifeSteal, 8f);
                    if (cfgShouldDropVolatileBattery)
                        PersonalizePodPlugin.SpawnBattery(characterBody.footPosition);
                }
            }
        }
    }
}