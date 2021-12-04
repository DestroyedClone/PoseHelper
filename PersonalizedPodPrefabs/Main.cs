using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace PersonalizedPodPrefabs
{
    [BepInPlugin("com.DestroyedClone.PersonalizedPodPrefabs", "Personalized Pod Prefabs", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Main : BaseUnityPlugin
    {
        public static Dictionary<BodyIndex, GameObject> bodyIndex_to_podPrefabs = new Dictionary<BodyIndex, GameObject>();
        public static GameObject genericPodPrefab;
        public static GameObject roboCratePodPrefab;

        public void Start()
        {
            genericPodPrefab = RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            roboCratePodPrefab = RoR2Content.Survivors.Toolbot.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;


        }
    }
}
