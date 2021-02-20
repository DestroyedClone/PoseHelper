using BepInEx;
using R2API.Utils;
using R2API;
using UnityEngine.Networking;
using RoR2;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;

namespace LeBuilder
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
   nameof(ItemAPI),
   nameof(BuffAPI),
   nameof(LanguageAPI),
   nameof(ResourcesAPI),
   nameof(PlayerAPI),
   nameof(PrefabAPI),
   nameof(SoundAPI),
   nameof(OrbAPI),
   nameof(NetworkingAPI),
   nameof(EffectAPI),
   nameof(EliteAPI),
   nameof(LoadoutAPI),
   nameof(SurvivorAPI),

        //scene building
   nameof(DirectorAPI)
   )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "LeBuilder";
        public const string ModGuid = "com.DestroyedClone.LeBuilder";

        internal static BepInEx.Logging.ManualLogSource _logger;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
            Hooks();
        }

        private void Hooks()
        {
        }


    }
}
