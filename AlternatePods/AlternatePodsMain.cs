using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
//dotnet build --configuration Release
[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace AlternatePods
{
    [BepInPlugin("com.DestroyedClone.AlternatePods", "Alternate Pods", "0.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    //[BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class AlternatePodsPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;
        internal static ConfigFile _config;
        public static AlternatePodsPlugin instance;
        
        public static event Action<VehicleSeat, GameObject> onPodLandedServer;
        public static event Action<VehicleSeat, GameObject> onRoboPodLandedServer;

        public static GameObject genericPodPrefab;
        public static GameObject roboCratePodPrefab;

        public static Dictionary<string,GameObject> podName_to_podPrefab = new Dictionary<string, GameObject>();

        private void Start() {
            _logger = Logger;
            _config = Config;
            instance = this;
            roboCratePodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion();
            genericPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();

            On.RoR2.Run.HandlePlayerFirstEntryAnimation += ReassignPodPrefab;
            //
            //
            new CommandoMain().Init(_config);
            
        }

        public GameObject LoadPod(bool isRoboPod)
        {
            //return isRoboPod ? 
        }

        public void ReassignPodPrefab(On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig, Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var cachedPodPrefab = body.preferredPodPrefab;
            if (body)
            {
                var pointer = body.GetComponent<PodModGenericSkillPointer>();
                if (pointer && pointer.podmodGenericSkill)
                {
                    var podName = pointer.podmodGenericSkill.skillName;
                    var podPrefab = podName_to_podPrefab.TryGetValue(podName, out GameObject requestedPodPrefab);
                    if (podPrefab)
                    {
                        body.preferredPodPrefab = requestedPodPrefab;
                    }
                }
            }
            orig(self, body, spawnPosition, spawnRotation);
            body.preferredPodPrefab = cachedPodPrefab;
        }

        public class PodModGenericSkillPointer : MonoBehaviour
        {
            public GenericSkill podmodGenericSkill = null;
        }

        /// <summary>
        /// A helper to easily set up and initialize an pod from your pod classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="configFile">The configuration file from the main plugin."</param>
        /// <param name="podBase">A new instance of an PodBase class."</param>
        public static bool ValidatePod(BepInEx.Configuration.ConfigFile configFile, PodBase podBase)
        {
            var survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.FindBodyPrefab(podBase.BodyName));
            if (survivorDef != null)
            {
                var enabled = configFile.Bind<bool>(podBase.ConfigCategory, "Enable Pod Modification?", true, "[Server] Should this body's pod get modified?").Value;
                if (enabled)
                {
                    return true;
                }
            }
            return false;
        }

    }
    public class Assets
    {
        //Prefabs
        public static GameObject genericPodPrefab;
        public static GameObject roboCratePodPrefab;
        public static GameObject batteryQuestPrefab;
    }
    public class ModCompat
    {
        public static bool starstormInstalled = false;
    }
}