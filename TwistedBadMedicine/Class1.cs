
using System.Collections.Generic;
using BepInEx;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using static On.RoR2.Achievements.Croco.CrocoKillScavengerAchievement.CrocoKillScavengerServerAchievement;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace TwistedBadMedicine
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class Class1 : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Twisted Bad Medicine";
        public const string ModGuid = "com.DestroyedClone.TwistedBadMedicine";

        public void Awake()
        {
            OnInstall += Class1_OnInstall;
            OnCharacterDeathGlobal += Class1_OnCharacterDeathGlobal;
        }

        public List<BodyIndex> allowedBodyIndicies = null;

        private void Class1_OnInstall(orig_OnInstall orig, RoR2.Achievements.BaseServerAchievement self)
        {
            orig(self);
            allowedBodyIndicies = new List<BodyIndex>
            {
                BodyCatalog.FindBodyIndex("ScavLunar1Body"),
                BodyCatalog.FindBodyIndex("ScavLunar2Body"),
                BodyCatalog.FindBodyIndex("ScavLunar3Body"),
                BodyCatalog.FindBodyIndex("ScavLunar4Body")
            };
        }

        private void Class1_OnCharacterDeathGlobal(orig_OnCharacterDeathGlobal orig, RoR2.Achievements.BaseServerAchievement self, RoR2.DamageReport damageReport)
        {
            orig(self, damageReport);
            if (allowedBodyIndicies.Contains(damageReport.victimBodyIndex) && self.serverAchievementTracker.networkUser.master == damageReport.attackerMaster)
            {
                self.Grant();
            }
        }
    }
}
