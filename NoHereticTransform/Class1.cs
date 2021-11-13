using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;

namespace NoHereticTransform
{
    [BepInPlugin("com.DestroyedClone.NoHereticTransform", "No Heretic Transform", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> cfgRequireBeads;

        public void Awake()
        {
            cfgRequireBeads = Config.Bind("", "Require Beads to Transform", true, "If true, then you will transform into the Heretic only if you have the full Heresy set and a Beads of Fealty.");

            On.RoR2.CharacterMaster.TransformBody += CharacterMaster_TransformBody;
        }

        private void CharacterMaster_TransformBody(On.RoR2.CharacterMaster.orig_TransformBody orig, CharacterMaster self, string bodyName)
        {
            if (bodyName == "HereticBody")
            {
                if (self.inventory && self.inventory.GetItemCount(RoR2Content.Items.LunarTrinket) > 0)
                {
                    orig(self, bodyName);
                }
                return;
            }
        }
    }
}