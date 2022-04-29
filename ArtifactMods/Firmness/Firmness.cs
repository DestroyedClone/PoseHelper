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

namespace ArtifactNoFlinch
{
    [BepInPlugin("com.DestroyedClone.FirmArtifact", "Artifact of Firmness", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(ArtifactAPI))]
    public class FirmnessArtifact : BaseUnityPlugin
    {
        public static ArtifactDef Firmness = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ArtifactDef EvolRef = Resources.Load<ArtifactDef>("artifactdefs/MonsterTeamGainsItems");

        public void Awake()
        {
            InitializeArtifact();
            On.RoR2.SetStateOnHurt.Start += SetStateOnHurt_Start;
        }

        private void SetStateOnHurt_Start(On.RoR2.SetStateOnHurt.orig_Start orig, SetStateOnHurt self)
        {
            orig(self);
            if (RunArtifactManager.instance.IsArtifactEnabled(Firmness))
                self.canBeHitStunned = false;
        }

        public static void InitializeArtifact()
        {
            Firmness.nameToken = "Artifact of Firmness";
            Firmness.descriptionToken = "Prevents monsters from flinching.";
            Firmness.smallIconDeselectedSprite = EvolRef.smallIconDeselectedSprite;
            Firmness.smallIconSelectedSprite = EvolRef.smallIconSelectedSprite;
            ArtifactAPI.Add(Firmness);
        }
    }
}