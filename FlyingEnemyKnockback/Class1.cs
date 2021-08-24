using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AggressionArtifact
{
    [BepInPlugin("com.DestroyedClone.FlyingEnemyKnockback", "FlyingEnemyKnockback", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(ArtifactAPI))]
    public class HPATPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            var com = self.gameObject.GetComponent<Rigidbody>();
            if (com)
            {
                if (!com.isKinematic) //flying check
                {
                    com.mass = 99999999;
                }
            }
        }
    }
}
