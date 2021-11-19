using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using static CollisionLODOverride.Methods;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CollisionLODOverride
{
    [BepInPlugin("com.DestroyedClone.CollisionLODOverride", "Collision LOD Override", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Main : BaseUnityPlugin
    {
        public static ConfigEntry<int> cfgLODOverride;
        public static int lodOverrideValue = 0;
        public static bool discoveryMode = false;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Start()
        {
            _logger = Logger;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            cfgLODOverride = Config.Bind("", "LOD Override Level", 2, "I'm basing them off how I felt they looked, not anything technical." +
                "\n" +
                "\n0 = Normal" +
                "\n1 = Simpler" +
                "\n2 = Simplest" +
                "\n3 = Leaves are solid as you approach, sometimes trunks are just invisible." +
                "\n-1/Negative numbers = Leaves are almost invisible until you approach them.");
            SetConfigSetting(cfgLODOverride.Value);

            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            On.RoR2.SettingsConVars.MaximumLodConVar.SetString += MaximumLodConVar_SetString;

            if (discoveryMode)
                On.RoR2.SceneDirector.PopulateScene += Discover;
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            string[] chosenPathSet = GetPathSet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            PatchScene(chosenPathSet, cfgLODOverride.Value);
        }

        private void MaximumLodConVar_SetString(On.RoR2.SettingsConVars.MaximumLodConVar.orig_SetString orig, RoR2.ConVar.BaseConVar self, string newValue)
        {
            orig(self, newValue);
            string[] chosenPathSet = GetPathSet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            PatchScene(chosenPathSet, cfgLODOverride.Value);
        }

        private void Discover(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            PrintSceneCollisions();
        }

        [ConCommand(commandName = "collision_lod_override_preview", flags = ConVarFlags.None, helpText = "collision_lod_override_preview {0, 1, 2, 3, -1} - Temporarily overrides the collideable LODs in scene.")]
        private static void ModifyPresetScenePreview(ConCommandArgs args)
        {
            var value = args.GetArgInt(0);
            var pathSet = GetPathSet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            if (pathSet == null)
            {
                _logger.LogWarning($"Could not find chosen pathSet for current scene ({UnityEngine.SceneManagement.SceneManager.GetActiveScene().name})!");
                return;
            }
            PatchScene(pathSet, value);
        }

        [ConCommand(commandName = "collision_lod_override", flags = ConVarFlags.None, helpText = "collision_lod_override {0, 1, 2, 3, -1} - Temporarily overrides the collideable LODs in scene.")]
        private static void ModifyPresetScene(ConCommandArgs args)
        {
            var value = args.GetArgInt(0);
            SetConfigSetting(value);
            var pathSet = GetPathSet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            if (pathSet == null)
            {
                _logger.LogWarning($"Could not find chosen pathSet for current scene ({UnityEngine.SceneManagement.SceneManager.GetActiveScene().name})!");
                return;
            }
            PatchScene(pathSet, value);
        }

        [ConCommand(commandName = "collision_lod_print", flags = ConVarFlags.None, helpText = "collision_lod_print {printPath:True/False} - Prints the scenename followed by the amount of collideable LOD groups, uncollideable LOD groups, and the total count." +
            "\nBoolean check to print the paths of the LOD objects for easy indexing to author.")]
        private static void DiscoverScene(ConCommandArgs args)
        {
            PrintSceneCollisions(args.GetArgBool(0));
        }
    }
}