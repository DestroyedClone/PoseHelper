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
        public static AlternatePodsPlugin instance;
        
        public static event Action<VehicleSeat, GameObject> onPodLandedServer;
        public static event Action<VehicleSeat, GameObject> onRoboPodLandedServer;

        private void Start() {
            instance = this;
            genericPodPrefab = RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            roboCratePodPrefab = RoR2Content.Survivors.Toolbot.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
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