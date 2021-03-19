using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace StageVariants
{
    [BepInPlugin("com.DestroyedClone.StageVariantsPlus", "Stage Variants Plus", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class SVPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.SceneDirector.Start += ChooseSceneToModify;
        }

        private void ChooseSceneToModify(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
        }
    }
}
