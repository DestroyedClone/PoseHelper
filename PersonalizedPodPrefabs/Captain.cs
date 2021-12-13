using R2API;
using RoR2;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class CaptainTemplate : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Captain.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<CaptainPodComponent>();
            return podPrefab;
        }

        public class CaptainPodComponent : PodComponent
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
                {
                    PersonalizePodPlugin.BuffTeam(passenger, RoR2Content.Buffs.ElephantArmorBoost, 11f);
                }
            }
        }
    }
}