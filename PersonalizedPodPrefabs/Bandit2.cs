using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Bandit2 : PodBase
    {
        public override string BodyName => "Bandit2Body";

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<Bandit2PodComponent>();
            return podPrefab;
        }

        public class Bandit2PodComponent : PodComponent
        {
            //private readonly float buffDuration = 10f;
            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (!NetworkServer.active) return;
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    var entityStateMachine = characterBody.GetComponents<EntityStateMachine>().FirstOrDefault(esm => esm.customName == "Stealth");
                    if (entityStateMachine != null)
                    {
                        entityStateMachine.SetNextState(new EntityStates.Bandit2.StealthMode());
                    }
                    //characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, buffDuration);
                }
            }
        }
    }
}