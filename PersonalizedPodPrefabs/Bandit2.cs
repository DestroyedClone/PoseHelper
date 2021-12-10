using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using RoR2.Projectile;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;
using UnityEngine.Networking;

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
            private readonly float buffDuration = 10f;
            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (!NetworkServer.active) return;
                var characterBody = passenger.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, buffDuration);
                }
            }
        }
    }
}
