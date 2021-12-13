using R2API;
using RoR2;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Engineer : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Engi.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<EngineerPodComponent>();
            return podPrefab;
        }

        public class EngineerPodComponent : PodComponent
        {
            protected override void Start()
            {
                addLandingAction = false;
                addExitAction = true;
                base.Start();
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (isServer)
                    BuffTeam(passenger, RoR2Content.Buffs.EngiTeamShield, 8f);
            }
        }

        public class EngineerPodComponentMissiles : PodComponent
        {
            private EquipmentSlot equipmentSlot;

            private readonly int maxMissileCount = 16;
            private int currentMissileCount = 1;
            private bool shouldFire = false;

            private float age = 0;
            private readonly float fireDelay = 0.5f;

            protected override void Start()
            {
                addLandingAction = false;
                base.Start();
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody && characterBody.equipmentSlot)
                {
                    shouldFire = true;
                    equipmentSlot = characterBody.equipmentSlot;
                    currentMissileCount = maxMissileCount;
                }
            }

            private void FixedUpdate()
            {
                age += Time.fixedDeltaTime;
                if (age >= fireDelay && shouldFire && currentMissileCount > 0)
                {
                    equipmentSlot.FireMissile();
                    currentMissileCount--;
                    age = 0;
                }
            }
        }
    }
}