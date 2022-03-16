using System;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace EngiProjectileTeamSwapSkill
{
    [BepInPlugin("com.DestroyedClone.EngiTrickShield", "Engi Trick Shield", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(new[] { nameof(PrefabAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(LoadoutAPI) })]
    public class Class1 : BaseUnityPlugin
    {
        internal static string modkeyword = "DC_ENGITRICKSHIELD";
        public static ConfigFile _config;
        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _config = Config;
            _logger = Logger;
            AssemblySetup();
        }

        public static void AssemblySetup() //credit to bubbet for base code
        {
            var survivorMainType = typeof(SurvivorMain);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract)
                {
                    if (survivorMainType.IsAssignableFrom(type))
                    {
                        var objectInitializer = (SurvivorMain)Activator.CreateInstance(type);
                        objectInitializer.Init(_config);
                    }
                }
            }
        }
    }
}
