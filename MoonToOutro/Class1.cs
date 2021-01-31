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

namespace MoonToOutro
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
       nameof(ResourcesAPI),
       nameof(PlayerAPI),
       nameof(PrefabAPI),
       nameof(NetworkingAPI)
       )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Immediate Moon To Outro";
        public const string ModGuid = "com.DestroyedClone.MoonToOutro";
    }

    private void Awake()
    {

    }
}
