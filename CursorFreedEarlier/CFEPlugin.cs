using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CursorFreedEarlier
{
    [BepInPlugin("com.DestroyedClone.CursorFreedEarlier", "Cursor Freed Earlier", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class CFEPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.SplashScreenController.Start += SplashScreenController_Start;
        }

        private void SplashScreenController_Start(On.RoR2.SplashScreenController.orig_Start orig, SplashScreenController self)
        {
            self.gameObject.AddComponent<RoR2.UI.CursorOpener>();
            orig(self);
        }
    }
}
