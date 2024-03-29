﻿using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Bandit2 : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Bandit2.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<Bandit2PodComponent>();
            return podPrefab;
        }

        public class Bandit2PodComponent : PodComponent
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
                    var entityStateMachine = characterBody.GetComponents<EntityStateMachine>().FirstOrDefault(esm => esm.customName == "Stealth");
                    if (entityStateMachine != null)
                    {
                        entityStateMachine.SetNextState(new EntityStates.Bandit2.StealthMode());
                    }
                }
            }
        }
    }
}