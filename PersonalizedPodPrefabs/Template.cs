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
    public class Template : PodBase
    {
        public override string BodyName => "Body";

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, PodPrefabName);
            podPrefab.AddComponent<PodComponent>();
            return podPrefab;
        }

        public class PodComponent : PodComponent
        {

        }
    }
}
