﻿using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.CompilerServices;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CptBustedDiabloAddOn
{
    [BepInPlugin("com.DestroyedClone.DiabloStrikeVisualFix", "DiabloStrikeVisualFix", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    //[BepInDependency(AncientScepter.AncientScepterMain.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static bool scepterIsLoaded = false;
        public static GameObject airstrikeGhostPrefab = Resources.Load<GameObject>("prefabs/projectileghosts/CaptainAirstrikeAltGhost");
        public static GameObject airstrikeProjectilePrefab = Resources.Load<GameObject>("prefabs/projectiles/captainairstrikealtprojectile");

        public void Start()
        {
            ModifyPrefab();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter"))
            {
                scepterIsLoaded = true;
            }
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;

            if (scepterIsLoaded)
            {
                //ModifyScepter();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ModifyScepter()
        {
            GameObject scepterProjectile = AncientScepter.CaptainAirstrikeAlt2.airstrikePrefab;
            if (scepterProjectile)
            {
                if (!scepterProjectile.GetComponent<AirstrikeVisualModifierProjectile>())
                {
                    scepterProjectile.AddComponent<AirstrikeVisualModifierProjectile>();
                }
            }


            GameObject scepterProjectileGhost = AncientScepter.CaptainAirstrikeAlt2.airstrikePrefab.GetComponent<ProjectileController>().ghostPrefab;
            if (scepterProjectileGhost)
            {
                var comghost = scepterProjectileGhost.GetComponent<AirstrikeVisualModifierGhost>();
                if (!comghost)
                {
                    comghost = scepterProjectileGhost.AddComponent<AirstrikeVisualModifierGhost>();
                }

                var areaIndicatorCenter = scepterProjectileGhost.transform.Find("AreaIndicatorCenter");
                List<ObjectTransformCurve> objectTransformCurves = new List<ObjectTransformCurve>();
                foreach (Transform child in areaIndicatorCenter)
                {
                    if (child.name == "LaserRotationalOffset" || child.name == "LaserRotationOffsetExtra")
                    {
                        var verticalOffset = child.Find("LaserVerticalOffset");
                        var laser = verticalOffset.Find("Laser");
                        objectTransformCurves.Add(laser.GetComponent<ObjectTransformCurve>());
                    }
                }

                comghost.laserScaleCurves = objectTransformCurves.ToArray();
            }
        }

        public void ModifyPrefab()
        {
            var comghost = airstrikeGhostPrefab.AddComponent<AirstrikeVisualModifierGhost>();
            comghost.indicatorRingScaleCurve = airstrikeGhostPrefab.transform.Find("AreaIndicatorCenter/IndicatorRing").GetComponent<ObjectScaleCurve>();
            var airstrikeOrientation = airstrikeGhostPrefab.transform.Find("AirstrikeOrientation");
            comghost.fallingProjectileScaleCurve = airstrikeOrientation.Find("FallingProjectile").GetComponent<ObjectTransformCurve>();

            var areaIndicatorCenter = airstrikeGhostPrefab.transform.Find("AreaIndicatorCenter");
            List<ObjectTransformCurve> objectTransformCurves = new List<ObjectTransformCurve>();
            foreach (Transform child in areaIndicatorCenter)
            {
                if (child.name == "LaserRotationalOffset")
                {
                    var verticalOffset = child.Find("LaserVerticalOffset");
                    var laser = verticalOffset.Find("Laser");
                    objectTransformCurves.Add(laser.GetComponent<ObjectTransformCurve>());
                }
            }
            comghost.laserScaleCurves = objectTransformCurves.ToArray();

            var com = airstrikeProjectilePrefab.AddComponent<AirstrikeVisualModifierProjectile>();
            com.projectileImpactExplosion = airstrikeProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            com.projectileController = com.GetComponent<ProjectileController>();

            airstrikeProjectilePrefab.GetComponent<ProjectileController>().allowPrediction = true;
        }

        public class AirstrikeVisualModifierGhost : MonoBehaviour
        {
            public ObjectScaleCurve indicatorRingScaleCurve;
            public ObjectTransformCurve fallingProjectileScaleCurve;
            public ObjectTransformCurve[] laserScaleCurves;

            public void UpdateVisuals(float timeMax)
            {
                indicatorRingScaleCurve.timeMax = timeMax;
                fallingProjectileScaleCurve.timeMax = timeMax * 0.55f;
                foreach (var laserScaleCurve in laserScaleCurves)
                {
                    laserScaleCurve.timeMax = timeMax;
                }
            }
        }

        public class AirstrikeVisualModifierProjectile : MonoBehaviour
        {
            public ProjectileImpactExplosion projectileImpactExplosion;
            public ProjectileController projectileController;
            public AirstrikeVisualModifierGhost airstrikeVisualModifierGhost;

            public void Start()
            {
                airstrikeVisualModifierGhost = projectileController.ghost.GetComponent<AirstrikeVisualModifierGhost>();
                if (!airstrikeVisualModifierGhost)
                    Chat.AddMessage("wtf");
                airstrikeVisualModifierGhost.UpdateVisuals(projectileImpactExplosion.lifetime);
            }
        }
    }
}