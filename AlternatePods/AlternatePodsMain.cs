using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;

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

    #region Compats

    [BepInDependency("com.Gnome.ChefMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.EnforcerGang.Enforcer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.HAND_Overclocked", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.DiggerUnearthed", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.Paladin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.SniperClassic", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.RegigigasMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TheTimeSweeper.TeslaTrooper", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.HenryMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.DiggerUnearthed", BepInDependency.DependencyFlags.SoftDependency)]
    public class AlternatePodsPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;
        internal static ConfigFile _config;

        //wait, why the fuck am I instancing?
        public static AlternatePodsPlugin instance;

        //public static event Action<VehicleSeat, GameObject> onPodLandedServer;
        //public static event Action<VehicleSeat, GameObject> onRoboPodLandedServer;

        public static Dictionary<string, GameObject> podName_to_podPrefab = new Dictionary<string, GameObject>();

        public static Dictionary<SkillDef, GameObject> skillDef_to_gameObject = new Dictionary<SkillDef, GameObject>();

        public static ConfigEntry<bool> cfgAddMonsterPods;
        public static ConfigEntry<bool> cfgListPodsOnStartup;
        public static ConfigEntry<bool> cfgEnableAchievements;

        private void Start()
        {
            _logger = Logger;
            _config = Config;
            instance = this;
            SetupConfig();

            Assets.SetupAssets();
            ModCompat.SetupModCompat();

            //CommandoACTUALDRAFTMain.Init();
            On.RoR2.Run.HandlePlayerFirstEntryAnimation += ReassignPodPrefab;
            //
            //
            AssemblySetup();
            if (cfgListPodsOnStartup.Value)
                OutputAvailablePods();
        }

        public static void SetupConfig()
        {
            cfgAddMonsterPods = _config.Bind("Survivor Pods", "Load Monster Pods", false, "If true, then any custom pods that are available for monsters will be loaded."
            + "\nIf you don't have a way to play as a monster, you should disable this.");
            cfgListPodsOnStartup = _config.Bind("Logging", "List Available Pods On Startup", true, "If true, the the mod will list the available custom survivor pods on startup.");
            cfgEnableAchievements = _config.Bind("Achievements", "Enable Achievements", true, "If true, then unlock conditions for pods will be activated.");
        }

        public void OutputAvailablePods()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"Available Pods:");
            int i = 0;
            foreach (var pair in podName_to_podPrefab)
            {
                stringBuilder.AppendLine($"[{i++}] {pair.Key} - {pair.Value}");
            }
            _logger.LogMessage(stringBuilder.ToString());
        }

        public void AssemblySetup() //credit to bubbet for base code
        {
            var survivorMainType = typeof(PodModCharBase);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract)
                {
                    if (survivorMainType.IsAssignableFrom(type))
                    {
                        var objectInitializer = (PodModCharBase)Activator.CreateInstance(type);
                        objectInitializer.Init();
                    }
                }
            }
        }

        public void ReassignPodPrefab(On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig, Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var cachedPodPrefab = body.preferredPodPrefab;
            if (body)
            {
                var pointer = body.GetComponent<PodModGenericSkillPointer>();
                if (pointer && pointer.podmodGenericSkill)
                {
                    var podName = pointer.podmodGenericSkill.skillDef.skillName;
                    //_logger.LogMessage($"User has pointer, skillName: {podName}");
                    if (podName == "PODMOD_SHARED_NOPOD")
                    {
                        //_logger.LogMessage("Podname is nopod, not replacing.");
                    }
                    else
                    {
                        var podPrefab = podName_to_podPrefab.TryGetValue(podName, out GameObject requestedPodPrefab);
                        //var podPrefab = skillDef_to_gameObject()
                        if (podPrefab)
                        {
                            //Logger.LogMessage("Replacing generic pod with "+requestedPodPrefab.name);
                            body.preferredPodPrefab = requestedPodPrefab;
                        }
                        else
                        {
                            _logger.LogWarning("Couldn't find podprefab for chosen pod name!");
                        }
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
}