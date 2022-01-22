using BepInEx;
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

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace BanditItemlet
{
    [BepInPlugin("com.DestroyedClone.Banditlet", "Banditlet", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public static BodyIndex banditBodyIndex;

        public void Start()
        {
            On.RoR2.GenericPickupController.BodyHasPickupPermission += GenericPickupController_BodyHasPickupPermission;
        }

        private bool GenericPickupController_BodyHasPickupPermission(On.RoR2.GenericPickupController.orig_BodyHasPickupPermission orig, CharacterBody body)
        {
            return orig(body) && !(body.bodyIndex == banditBodyIndex);
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.BodyCatalog))]
        private static void CacheBodyCata()
        {
            banditBodyIndex = BodyCatalog.FindBodyIndex("Bandit2Body");
            Debug.LogError($"{BodyCatalog.FindBodyIndex("Bandit2Body")} {(int)BodyCatalog.FindBodyIndex("Bandit2Body")}");
        }

    }
}
