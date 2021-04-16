using BepInEx;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CloakHidesHealth
{
    [BepInPlugin("com.DestroyedClone.CloakHidesHealth", "Cloak Hides Health", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class CloakHidesHealthPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += CombatHealthBarViewer_VictimIsValid;
        }

        private bool CombatHealthBarViewer_VictimIsValid(On.RoR2.UI.CombatHealthBarViewer.orig_VictimIsValid orig, RoR2.UI.CombatHealthBarViewer self, HealthComponent victim)
        {
            bool isInvis = victim.body && victim.body.hasCloakBuff;

            return victim && victim.alive && (self.victimToHealthBarInfo[victim].endTime > Time.time || victim == self.crosshairTarget) && !isInvis;
        }

    }
}
