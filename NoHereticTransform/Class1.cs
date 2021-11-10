using BepInEx;
using R2API.Utils;
using RoR2;

namespace NoHereticTransform
{
    [BepInPlugin("com.DestroyedClone.NoHereticTransform", "No Heretic Transform", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.CharacterMaster.TransformBody += CharacterMaster_TransformBody;
        }

        private void CharacterMaster_TransformBody(On.RoR2.CharacterMaster.orig_TransformBody orig, CharacterMaster self, string bodyName)
        {
            if (bodyName == "HereticBody")
                return;
        }
    }
}