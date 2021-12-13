using R2API;
using RoR2;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Treebot : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Toolbot.bodyPrefab.name;
        public override bool ShouldAddVolatileBatteryHook => true;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, PodPrefabName);
            podPrefab.AddComponent<TreebotPodComponent>();
            return podPrefab;
        }

        public class TreebotPodComponent : PodComponent
        {
            protected override void Start()
            {
                addLandingAction = false;
                addExitAction = true;
                base.Start();
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (!isServer) return;

                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    characterBody.AddTimedBuff(RoR2Content.Buffs.Energized, 8f);
                    if (cfgShouldDropVolatileBattery)
                    {
                        SpawnBattery(characterBody.footPosition);
                    }
                }
            }
        }
    }
}