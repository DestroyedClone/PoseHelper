using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using Path = System.IO.Path;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace PersonalizedPodPrefabs
{
    [BepInPlugin("com.DestroyedClone.PersonalizedPodPrefabs", "Personalized Pod Prefabs", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    //[BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class PersonalizePodPlugin : BaseUnityPlugin
    {
        public static Dictionary<BodyIndex, GameObject> bodyIndex_to_podPrefabs = new Dictionary<BodyIndex, GameObject>();
        public static GameObject genericPodPrefab;
        public static GameObject roboCratePodPrefab;

        public static bool starstormInstalled = false;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static PersonalizePodPlugin instance;

        // enfucker

        public void Start()
        {
            instance = this;
            _logger = Logger;
            genericPodPrefab = RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            roboCratePodPrefab = RoR2Content.Survivors.Toolbot.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.SurvivorCatalog))]
        public static void AssemblyShit()
        {
            var PodTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(PodBase)));

            foreach (var podType in PodTypes)
            {
                PodBase podBase = (PodBase)Activator.CreateInstance(podType);
                if (ValidatePod(instance.Config, podBase))
                {
                    podBase.Init(instance.Config);
                }
            }


            var survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.FindBodyPrefab("EnforcerBody"));
            if (survivorDef)
            {
                _logger.LogMessage("Enforcer is loaded, setting him up.");
                Enfucker.Init(survivorDef);
                return;
            }
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
                    // whatever
                }
            }
            return false;
        }

        public class PodComponent : MonoBehaviour
        {
            public VehicleSeat vehicleSeat;

            protected virtual void Start()
            {
                if (!vehicleSeat)
                {
                    vehicleSeat = gameObject.GetComponent<SurvivorPodController>().vehicleSeat;
                }
                vehicleSeat.onPassengerExit += VehicleSeat_onPassengerExit;
            }

            protected virtual void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                
            }

            protected virtual void OnDestroy()
            {
                vehicleSeat.onPassengerExit -= VehicleSeat_onPassengerExit;
            }
        }
    }

}