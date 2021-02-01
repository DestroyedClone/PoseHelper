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

namespace PoseHelper
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
       nameof(SurvivorAPI)
       )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Pose Helper";
        public const string ModGuid = "com.DestroyedClone.PoseHelper";

        internal static BepInEx.Logging.ManualLogSource _logger;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
            Hooks();
        }

        private void MakeRadarScannerNotBright() //https://stackoverflow.com/questions/55013068/changing-prefabs-fields-from-script-unity
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner");
            prefab.GetComponent<ChestRevealer>().pulseEffectScale = 0f;

        }

        private void Hooks()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.ChestRevealer.Init += ChestRevealer_Init;
        }

        private void ChestRevealer_Init(On.RoR2.ChestRevealer.orig_Init orig)
        {
            orig();
            orig.Target.SetFieldValue<GameObject>("pulseEffectPrefab", null);
            Debug.Log("pulse effect prefab wiped");
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "lobby":
                    GameObject.Find("Directional Light").GetComponent<Light>().color = Color.white;
                    var localMaster = PlayerCharacterMasterController.instances[0].master;
                    if (localMaster)
                    {
                        localMaster.GetBody().characterMotor.Motor.SetPositionAndRotation(new Vector3(0.12f, 0.91f, 7.76f), Quaternion.identity, true);
                    }
                    break;
            }
        }

        private void CharacterBody_onBodyStartGlobal(RoR2.CharacterBody obj)
        {
            if (obj && obj.isPlayerControlled && obj.master)
            {
                if (!obj.masterObject.GetComponent<Commands.DesCloneCommandComponent>())
                    obj.masterObject.AddComponent<Commands.DesCloneCommandComponent>();
            }
        }
    }
}
