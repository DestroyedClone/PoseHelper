using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace PersonalizedPodPrefabs
{
    [BepInPlugin("com.DestroyedClone.PersonalizedPodPrefabs", "Personalized Pod Prefabs", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Main : BaseUnityPlugin
    {
        public static Dictionary<BodyIndex, GameObject> bodyIndex_to_podPrefabs = new Dictionary<BodyIndex, GameObject>();
        public static GameObject genericPodPrefab;
        public static GameObject roboCratePodPrefab;

        public static bool starstormInstalled = false;

        internal static BepInEx.Logging.ManualLogSource _logger;

        // enfucker

        public void Start()
        {
            _logger = Logger;
            genericPodPrefab = RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            roboCratePodPrefab = RoR2Content.Survivors.Toolbot.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;

            RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = RoR2Content.Survivors.Croco.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab;
            SetupModCompat();
            ModifyAcridPrefab();

            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            Init_ModdedSurvivors();
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        public void ModifyAcridPrefab()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(roboCratePodPrefab, "AcridPodPrefab");
            //podPrefab.GetComponent<VehicleSeat>().onPassengerExit += AcridPod_Exit;
            RoR2Content.Survivors.Croco.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = podPrefab;
            podPrefab.AddComponent<AcridPodComponent>();
        }

        private void SetupModCompat()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2"))
            {
                starstormInstalled = true;
            }
        }

        //[RoR2.SystemInitializer(dependencies: typeof(RoR2.ItemCatalog))]
        public static void Init_ModdedSurvivors()
        {
            var survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.FindBodyPrefab("EnforcerBody"));
            if (survivorDef)
            {
                _logger.LogMessage("Enforcer is loaded, setting him up.");
                Enfucker.Init(survivorDef);
                return;
            }
        }

        private void SetupChirr()
        {
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
        public class AcridPodComponent : PodComponent
        {
            protected override void VehicleSeat_onPassengerExit(GameObject obj)
            {
                Vector3 position = obj.transform.position;
                var characterBody = obj.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    position = characterBody.footPosition;
                }
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = EntityStates.Croco.BaseLeap.projectilePrefab,
                    crit = false,
                    force = 0f,
                    damage = characterBody ? characterBody.damage : 18f,
                    owner = obj,
                    rotation = Quaternion.identity,
                    position = position
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
    }

}