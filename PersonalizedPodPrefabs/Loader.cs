using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Loader : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Loader.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<LoaderPodComponent>();
            return podPrefab;
        }

        public class LoaderPodComponent : PodComponent
        {
            protected override void Start()
            {
                addLandingAction = false;
                base.Start();
                podController.exitAllowed = true;
            }

            public void Update()
            {
                vehicleSeat.exitPosition = vehicleSeat.seatPosition;
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (!NetworkServer.active) return;

                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    var entityStateMachine = characterBody.GetComponents<EntityStateMachine>().FirstOrDefault(esm => esm.customName == "Body");
                    if (entityStateMachine != null)
                    {
                        //TeleportHelper.TeleportBody(characterBody, characterBody.footPosition + Vector3.up * 700f);
                        entityStateMachine.SetNextState(new EntityStates.Loader.PreGroundSlam());
                        characterBody.characterMotor.velocity.y = -200f;
                    }

                    var podESM = podController.GetComponent<EntityStateMachine>();
                    if (podESM)
                    {
                        podESM.SetNextState(new EntityStates.SurvivorPod.PreRelease());
                    }
                }
            }
        }
    }
}