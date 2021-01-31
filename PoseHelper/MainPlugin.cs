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
            MakeRadarScannerNotBright();
        }

        private void MakeRadarScannerNotBright()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner");
            prefab.GetComponent<ChestRevealer>().pulseEffectPrefab = null;
        }

        private void Hooks()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "moon":
                    //var EscapeSequenceController = GameObject.Find("EscapeSequenceController");
                    //var EscapeSequenceObjects = EscapeSequenceController.transform.Find("EscapeSequenceObjects");
                    //EscapeSequenceObjects.gameObject.SetActive(true);
                    //GameObject.Find("Ending Trigger").transform.position = new Vector3(2654, 206, 723);
                    UnityEngine.Object.FindObjectOfType<EscapeSequenceController>().CompleteEscapeSequence();
                    break;
                case "outro":
                    GameObject.Find("CutsceneController").GetComponent<PlayableDirector>().initialTime = 40f;
                    break;
                case "lobby":
                    GameObject.Find("Directional Light").GetComponent<Light>().color = Color.white;
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
