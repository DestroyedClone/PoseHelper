using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Merc : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Merc.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<MercPodComponent>();
            return podPrefab;
        }

        public class MercPodComponent : PodComponent
        {
            protected override void Start()
            {
                addLandingAction = false;
                base.Start();
                podController.exitAllowed = true;
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (!isServer) return;
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    var entityStateMachine = characterBody.GetComponents<EntityStateMachine>().FirstOrDefault(esm => esm.customName == "Body");
                    if (entityStateMachine != null)
                    {
                        entityStateMachine.SetNextState(new EntityStates.Merc.FocusedAssaultDash());
                    }
                }
                var podESM = podController.GetComponent<EntityStateMachine>();
                if (podESM)
                {
                    podESM.SetNextState(new EntityStates.SurvivorPod.PreRelease());
                }
            }

            public void Update()
            {
                vehicleSeat.exitPosition = vehicleSeat.seatPosition;
            }
        }
    }
}