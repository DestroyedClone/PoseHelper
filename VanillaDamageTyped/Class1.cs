using BepInEx;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using UnityEngine.UI;
using TMPro;
using R2API;
using RoR2.UI;
using UnityEngine.Networking;
using static R2API.DamageAPI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace VanillaDamageTyped
{
    [BepInPlugin("com.DestroyedClone.VanillaDamageTyped", "Vanilla Damage Type", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(DamageAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static ModdedDamageType dt_melee;
        public static ModdedDamageType dt_ranged;
        public static ModdedDamageType dt_bullet;
        public static ModdedDamageType dt_explosive;
        public static ModdedDamageType dt_fire;
        public static ModdedDamageType dt_fall;

        public static int damageColorOverride = -1;

        public static Color[] additionalDamageColors = new Color[]
        {
            new Color(0.113f, 0.050f, 0.647f), //blue
            new Color(0,0,0) //black
        };

        public void Awake()
        {
            RegisterDamageTypes();

            On.RoR2.DamageColor.FindColor += AddAdditionalColors;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo)
        {
            fireProjectileInfo.damageColorIndex = (DamageColorIndex)damageColorOverride;
            orig(self, fireProjectileInfo);
        }

        private Color AddAdditionalColors(On.RoR2.DamageColor.orig_FindColor orig, DamageColorIndex colorIndex)
        {
            if (colorIndex < DamageColorIndex.Default || colorIndex >= DamageColorIndex.Count)
            {
                int extraValue = colorIndex - DamageColorIndex.Count;
                if (colorIndex >= DamageColorIndex.Count && extraValue <= additionalDamageColors.Length)
                {
                    return additionalDamageColors[extraValue];
                }
                return Color.white;
            }
            return DamageColor.colors[(int)colorIndex];
        }

        public void RegisterDamageTypes()
        {
        }
        public void AddDamageTypeToProjectile()
        {
            
        }

        [ConCommand(commandName = "dco", flags = ConVarFlags.ExecuteOnServer, helpText = "dco {index}")]
        private static void A(ConCommandArgs args)
        {
            damageColorOverride = args.GetArgInt(0);
        }
    }
}
