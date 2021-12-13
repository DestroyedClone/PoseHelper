using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Huntress : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Huntress.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<HuntressPodComponent>();
            return podPrefab;
        }

        public class HuntressPodComponent : PodComponent
        {
            protected override void Start()
            {
                addLandingAction = false;
                addExitAction = true;
                base.Start();
                podController.exitAllowed = true;
            }

            public void FixedUpdate()
            {
                vehicleSeat.exitPosition = vehicleSeat.seatPosition;
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (isServer)
                {
                    var characterBody = passenger.GetComponent<CharacterBody>();
                    if (characterBody)
                    {
                        var entityStateMachine = characterBody.GetComponents<EntityStateMachine>().FirstOrDefault(esm => esm.customName == "Body");
                        if (entityStateMachine != null)
                        {
                            //TeleportHelper.TeleportBody(characterBody, characterBody.footPosition + Vector3.up * 700f);
                            entityStateMachine.SetNextState(new EntityStates.Huntress.MiniBlinkState());
                            //characterBody.characterMotor.velocity.y = -200f;
                        }
                        //characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, buffDuration);
                    }
                    var podESM = podController.GetComponent<EntityStateMachine>();
                    if (podESM)
                    {
                        podESM.SetNextState(new EntityStates.SurvivorPod.PreRelease());
                    }
                }

                enabled = false;
            }
        }
    }
}