using R2API;
using RoR2;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Commando : PodBase
    {
        public override string BodyName => RoR2Content.Survivors.Commando.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<CommandoPodComponent>();
            return podPrefab;
        }

        public class CommandoPodComponent : PodComponent
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
                    characterBody.AddTimedBuff(RoR2Content.Buffs.Energized, 8f);
            }
        }
    }
}