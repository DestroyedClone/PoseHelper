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

namespace ROR1AltSkills
{
    [BepInPlugin("com.DestroyedClone.OriginalSkills", "Original Skills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(DamageAPI), nameof(BuffAPI), nameof(DotAPI))]
    public class OriginalSkillsPlugin : BaseUnityPlugin
    {
        internal static string modkeyword = "DC_ORIGSKILLS_KEYWORD_IDENTIFIER";

        public void Awake()
        {
            SetupLanguage();

            Acrid.AcridMain.Init();
            //Commando.CommandoMain.Init();
            //Huntress.HuntressMain.Init();
            Loader.LoaderMain.Init(Config);
        }

        public void SetupLanguage()
        {
            LanguageAPI.Add(modkeyword, $"[ Original Skills Mod ]");
        }
    }
}
