using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using System.Security;
using System.Security.Permissions;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AlternateSkills
{
    [BepInPlugin("com.DestroyedClone.AlternateSkills", "Alternate Skills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI))]

    public class MainPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            //Buffs.RegisterBuffs();
            //Acrid.AcridMain.Init();
            Captain.CaptainMain.Init();
            //Commando.CommandoMain.Init();
        }

    }
}
