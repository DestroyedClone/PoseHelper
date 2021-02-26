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
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

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
            GameObject prefab = Resources.Load<GameObject>("prefabs/effects/ActivateRadarTowerEffect");
            prefab.GetComponent<ChestRevealer>().pulseEffectScale = 0f;
        }

        private void Hooks()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.RoR2Application.Awake += RoR2Application_Awake;
        }

        private void RoR2Application_Awake(On.RoR2.RoR2Application.orig_Awake orig, RoR2Application self)
        {
            orig(self);
            if (!self.GetComponent<RadarScannerOverride>())
            {
                var component = self.gameObject.AddComponent<RadarScannerOverride>();
                component.prefab = Resources.Load<GameObject>("prefabs/effects/ActivateRadarTowerEffect");
                //component.prefab.transform.Find("PP").gameObject.SetActive(false);
                // component.prefab.GetComponent<DestroyOnTimer>().transform.Find("");
                component.prefab.transform.Find("PP").gameObject.SetActive(false);
                component.prefab.transform.Find("Point Light").gameObject.SetActive(false);
                component.prefab.GetComponent<DestroyOnTimer>().duration = 0f;
            }
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

        public class RadarScannerOverride : MonoBehaviour
        {
            public GameObject prefab;
        }
    }
}
