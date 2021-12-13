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
        public static GameObject batteryQuestPrefab;

        public static bool starstormInstalled = false;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static PersonalizePodPlugin instance;

        //Events
        public static event Action<VehicleSeat, GameObject> onPodLandedServer;
        public static event Action<VehicleSeat, GameObject> onRoboPodLandedServer;

        // enfucker

        public void Start()
        {
            instance = this;
            _logger = Logger;
            genericPodPrefab = RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            roboCratePodPrefab = RoR2Content.Survivors.Toolbot.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            SetupBattery();

            Hooks();
        }

        public static void SetupBattery()
        {
            var battery = Resources.Load<GameObject>("prefabs/networkedobjects/questvolatilebatteryworldpickup");
            batteryQuestPrefab = PrefabAPI.InstantiateClone(battery, "QuestVolatileBatteryWorldPickupPPP", true);
            //Destroy(batteryQuestPrefab.GetComponent<AwakeEvent>());
            //Destroy(batteryQuestPrefab.GetComponent<NetworkParent>());
            batteryQuestPrefab.GetComponent<GenericPickupController>().enabled = true;            batteryQuestPrefab.AddComponent<MakeAvailable>().genericPickupController = batteryQuestPrefab.GetComponent<GenericPickupController>();        }

        private class MakeAvailable : MonoBehaviour
        {
            public GenericPickupController genericPickupController;
            public float age;

            public void FixedUpdate()
            {
                age += Time.fixedDeltaTime;
                if (age > 1f)
                {
                    genericPickupController.enabled = true;
                    enabled = false;
                }
            }
        }

        private void Hooks()
        {
            //On.EntityStates.SurvivorPod.Landed.OnEnter += CallLandedAction;
            On.EntityStates.RoboCratePod.Descent.OnExit += Descent_OnExit;
        }

        private void Descent_OnExit(On.EntityStates.RoboCratePod.Descent.orig_OnExit orig, EntityStates.RoboCratePod.Descent self)
        {
            orig(self);
            if (!UnityEngine.Networking.NetworkServer.active) return;
            if (self.vehicleSeat)
            {
                Action<VehicleSeat, GameObject> action2 = onRoboPodLandedServer;
                if (action2 == null)
                {
                    return;
                }
                action2(self.vehicleSeat, self.vehicleSeat.passengerBodyObject);
            }
        }

        private void CallLandedAction(On.EntityStates.SurvivorPod.Landed.orig_OnEnter orig, EntityStates.SurvivorPod.Landed self)
        {
            orig(self);
            if (!UnityEngine.Networking.NetworkServer.active) return;
            if (self.vehicleSeat)
            {
                Action<VehicleSeat, GameObject> action2 = onPodLandedServer;
                if (action2 == null)
                {
                    return;
                }
                action2(self.vehicleSeat, self.vehicleSeat.passengerBodyObject);
            }
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.SurvivorCatalog))]
        public static void AssemblySetup()
        {
            var PodTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(PodBase)));
            int podAmount = 0;
            foreach (var podType in PodTypes)
            {
                PodBase podBase = (PodBase)Activator.CreateInstance(podType);
                if (ValidatePod(instance.Config, podBase))
                {
                    podBase.Init(instance.Config);
                    _logger.LogMessage("Added pod for " + podBase.BodyName);
                    podAmount++;
                }
            }
            _logger.LogMessage($"Amount of pod types added: " + podAmount);


            var survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.FindBodyPrefab("EnforcerBody"));
            if (survivorDef)
            {
                _logger.LogMessage("Enforcer is loaded, setting him up.");
                //Enfucker.Init(survivorDef);
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
                    return true;
                }
            }
            return false;
        }

        public static void BuffTeam(GameObject passenger, BuffDef buffDef, float duration, float durationSelf = -1)
        {
            var characterBody = passenger.GetComponent<CharacterBody>();
            if (!characterBody) return;
            if (durationSelf == -1) durationSelf = duration;
            TeamComponent[] array = UnityEngine.Object.FindObjectsOfType<TeamComponent>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].teamIndex == characterBody.teamComponent.teamIndex)
                {
                    var teamBody = array[i].GetComponent<CharacterBody>();
                    if (teamBody == characterBody)
                    {
                        teamBody.AddTimedBuff(buffDef, durationSelf);
                        continue;
                    }
                    teamBody.AddTimedBuff(buffDef, duration);
                }
            }
        }

        public static void SpawnBattery(Vector3 position)
        {
            var battery = UnityEngine.Object.Instantiate(batteryQuestPrefab, position, Quaternion.identity);
            UnityEngine.Networking.NetworkServer.Spawn(battery);
        }

        public class PodComponent : MonoBehaviour
        {
            public VehicleSeat vehicleSeat;
            public bool addExitAction = true;
            public bool addLandingAction = false;
            public bool roboCrateDropBattery = true;
            public SurvivorPodController podController;
            public bool isServer = false;

            protected virtual void Start()
            {
                isServer = UnityEngine.Networking.NetworkServer.active;
                if (!vehicleSeat)
                {
                    vehicleSeat = gameObject.GetComponent<SurvivorPodController>().vehicleSeat;
                }
                if (addExitAction)
                    vehicleSeat.onPassengerExit += VehicleSeat_onPassengerExit;
                if (addLandingAction)
                {
                    if (vehicleSeat.ejectOnCollision)
                    {
                        onRoboPodLandedServer += PersonalizePodPlugin_onPodLandedServer;
                        if (roboCrateDropBattery)
                            onRoboPodLandedServer += PodComponent_onRoboPodLandedServer;
                    }
                    else
                        PersonalizePodPlugin.onPodLandedServer += PersonalizePodPlugin_onPodLandedServer;
                }
                podController = vehicleSeat.GetComponent<SurvivorPodController>();
            }

            private void PodComponent_onRoboPodLandedServer(VehicleSeat actionVehicleSeat, GameObject passenger)
            {
                if (actionVehicleSeat.rigidbody == vehicleSeat.rigidbody)
                {
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex), vehicleSeat.exitPosition.position, Vector3.up);
                }
            }

            protected virtual void PersonalizePodPlugin_onPodLandedServer(VehicleSeat actionVehicleSeat, GameObject passengerBodyObject)
            {
            }

            protected virtual void VehicleSeat_onPassengerExit(GameObject passenger)
            { }

            protected virtual void OnDestroy()
            {
                if (addExitAction)
                    vehicleSeat.onPassengerExit -= VehicleSeat_onPassengerExit;
                if (addLandingAction)
                {
                    if (vehicleSeat.ejectOnCollision)
                    {
                        onRoboPodLandedServer -= PersonalizePodPlugin_onPodLandedServer;
                        if (roboCrateDropBattery)
                            onRoboPodLandedServer -= PodComponent_onRoboPodLandedServer;
                    }
                    else
                        PersonalizePodPlugin.onPodLandedServer -= PersonalizePodPlugin_onPodLandedServer;
                }
            }
        }
    }

}