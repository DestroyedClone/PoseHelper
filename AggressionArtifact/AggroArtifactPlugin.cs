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
    [BepInPlugin("com.DestroyedClone.AggressionArtifact", "Aggression Artifact", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(ArtifactAPI))]
    public class HPATPlugin : BaseUnityPlugin
    {
        public static ArtifactDef Aggression = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ConfigEntry<float> RangeMultiplier { get; set; }

        public void Awake()
        {
            RangeMultiplier = Config.Bind("", "Range Multiplier", -1f, "While the artifact is active, multiplies the range of how far the monster will scan for enemies." +
                "\nSet to negative for infinite range.");
            InitializeArtifact();
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
        }

        private HurtBox BaseAI_FindEnemyHurtBox(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(Aggression))
            {
                var range = RangeMultiplier.Value < 0 ? float.PositiveInfinity : maxDistance * RangeMultiplier.Value;
                return orig(self, range, true, filterByLoS);
            }
            return orig(self, maxDistance, full360Vision, filterByLoS);
        }

        public static void InitializeArtifact()
        {
            Aggression.nameToken = "Artifact of Aggression";
            Aggression.descriptionToken = "Increases range of monster targeting " + (RangeMultiplier.Value < 0 ? "infinitely." : ("by " + RangeMultiplier.Value * 100f + "%."));
            //Aggression.smallIconDeselectedSprite = AssetLoaderAndChecker.MainAssets.LoadAsset<Sprite>("Assets/Textures/Artifact/VarianceDisabled.png");
            //Aggression.smallIconSelectedSprite = AssetLoaderAndChecker.MainAssets.LoadAsset<Sprite>("Assets/Textures/Artifact/VarianceEnabled.png");
            ArtifactAPI.Add(Aggression);
        }
    }
}