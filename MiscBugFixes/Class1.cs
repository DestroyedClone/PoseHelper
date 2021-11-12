using UnityEngine;
using RoR2;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using System.Security;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using BepInEx;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MiscBugFixes
{
    [BepInPlugin("com.DestroyedClone.MiscBugFixes", "MiscBugFixes", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(DamageAPI), nameof(BuffAPI), nameof(DotAPI))]
    public class Class1 : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            On.RoR2.GenericSkill.RunRecharge += GenericSkill_RunRecharge;
        }

        private void GenericSkill_RunRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
        {
            if (!self.stateMachine || self.stateMachine.state == null)
            {
                //_logger.LogError("Caught: StateMachine or StateMachine.state returned null");
                return;
            }
            orig(self, dt);
        }
    }
}
