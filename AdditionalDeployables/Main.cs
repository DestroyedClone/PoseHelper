using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
//using static AdditionalDeployables.ServerTrackers;
using UnityEngine.AddressableAssets;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ProjectileLimiter
{
    [BepInPlugin("com.DestroyedClone.ProjectileLimiter", "Projectile Limiter", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(DeployableAPI), nameof(PrefabAPI))]
    public class Main : BaseUnityPlugin
    {
        //public static GameObject scanner = Resources.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner");
        //public static GameObject gateway = Resources.Load<GameObject>("Prefabs/NetworkedObjects/Zipline");
        public static GameObject saw;
        //public static GameObject blackhole = Resources.Load<GameObject>("Prefabs/Projectiles/GravSphere");
        public static GameObject meteorite;

        public static float cfgScannerCooldown;

        public static float cfgGatewayCooldown;
        //public static int cfgGatewayMax = 20;

        public static float cfgSawCooldown;
        public static int cfgSawMax;

        public static float cfgBlackholeCooldown;
        //public static int cfgBlackholeMax = 20;

        public static float cfgMeteoriteCooldown;
        public static float cfgMeteoriteMax;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public enum PerPlayerDeployableType
        {
            None,
            Saw,
            Blackhole,
            Meteorite
        }

        public void Awake()
        {
            _logger = Logger;
            SetupConfig();
            Overrides.RunOverrides();
            ModifyPrefabs();

            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;

            meteorite = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStorm.prefab").WaitForCompletion();
            saw = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Saw/Sawmerang.prefab").WaitForCompletion(); 
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (NetworkServer.active)
            {
                obj.gameObject.AddComponent<ProjectileDeployableTracker>();
            }
        }

        private void SetupConfig()
        {
            cfgScannerCooldown = Config.Bind("Radar Scanner", "Subcooldown", 5f, "").Value;

            cfgGatewayCooldown = Config.Bind("Eccentric Vase", "Subcooldown", 5f, "").Value;
            //cfgGatewayMax = Config.Bind("", "Max per Stage", 20, "").Value;

            cfgSawCooldown = Config.Bind("Sawmerang", "Subcooldown", 0f, "").Value;
            cfgSawMax = Config.Bind("Sawmerang", "Max per Player", 30, "").Value;

            cfgBlackholeCooldown = Config.Bind("Primordial Cube", "Subcooldown", 5f, "").Value;
            //cfgBlackholeMax = Config.Bind("", "Max per Stage", 20, "").Value;

            cfgMeteoriteCooldown = Config.Bind("Glowing Meteorite", "Subcooldown", 2.5f, "").Value;
            cfgMeteoriteMax = Config.Bind("Glowing Meteorite", "Max per Player", 10, "").Value;
        }

        public static void ModifyPrefabs()
        {
            /*if (scanner)
             *{
             *   var deployable = scanner.AddComponent<CustomDeployablePerServer>();
             *   deployable.deployableType = PerServerDeployableType.Scanner;
            }*/
            //if (gateway)
            //{
            //    var deployable = gateway.AddComponent<CustomDeployablePerServer>();
            //    deployable.deployableType = PerServerDeployableType.Gateway;
            //}
            if (saw)
            {
                var deployable = saw.AddComponent<CustomDeployablePerPlayer>();
                deployable.deployableType = PerPlayerDeployableType.Saw;
                deployable.projectileController = saw.GetComponent<RoR2.Projectile.ProjectileController>();
            }
            //if (blackhole)
            //{
            //    var deployable = blackhole.AddComponent<CustomDeployablePerPlayer>();
            //    deployable.deployableType = PerPlayerDeployableType.Blackhole;
            //}
            if (meteorite)
            {
                var deployable = meteorite.AddComponent<CustomDeployablePerPlayer>();
                deployable.deployableType = PerPlayerDeployableType.Meteorite;
                deployable.meteorStormController = meteorite.GetComponent<MeteorStormController>();
            }
        }

        // This goes on the projectile
        public class CustomDeployablePerPlayer : MonoBehaviour
        {
            public PerPlayerDeployableType deployableType = PerPlayerDeployableType.None;
            public ProjectileDeployableTracker deployableTracker;
            public List<GameObject> list = null;
            public RoR2.Projectile.ProjectileController projectileController;
            public MeteorStormController meteorStormController;

            public void Start()
            {
                if (!projectileController)
                {
                    projectileController = gameObject.GetComponent<RoR2.Projectile.ProjectileController>();
                }

                if (projectileController)
                {
                    var owner = projectileController.Networkowner;
                    if (owner)
                    {
                        deployableTracker = owner.GetComponent<ProjectileDeployableTracker>();
                    }
                }
                else if (meteorStormController)
                {
                    var owner = meteorStormController.owner;
                    if (owner)
                    {
                        deployableTracker = owner.GetComponent<ProjectileDeployableTracker>();
                    }
                }
                if (deployableTracker)
                {
                    switch (deployableType)
                    {
                        case PerPlayerDeployableType.Saw:
                            deployableTracker.sawList.Add(gameObject);
                            break;
                        case PerPlayerDeployableType.Meteorite:
                            deployableTracker.meteoriteList.Add(gameObject);
                            break;
                    }
                }
            }

            public void OnDisable()
            {
                if (deployableTracker)
                {
                    switch (deployableType)
                    {
                        case PerPlayerDeployableType.Saw:
                            deployableTracker.sawList.Remove(gameObject);
                            break;
                        case PerPlayerDeployableType.Meteorite:
                            deployableTracker.meteoriteList.Remove(gameObject);
                            break;
                    }
                }
            }
        }

        // This goes on the master
        public class ProjectileDeployableTracker : MonoBehaviour
        {
            public List<GameObject> sawList = new List<GameObject>();
            public List<GameObject> meteoriteList = new List<GameObject>();

            public void Start()
            {
            }

            public bool CheckSummonAvailability(PerPlayerDeployableType deployableType)
            {
                switch (deployableType)
                {
                    case PerPlayerDeployableType.Saw:
                        return sawList.Count < cfgSawMax;
                    case PerPlayerDeployableType.Meteorite:
                        return meteoriteList.Count < cfgMeteoriteMax;
                }
                return false;
            }
        }
    }
}