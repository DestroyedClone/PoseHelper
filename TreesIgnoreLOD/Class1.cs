using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using static TreesIgnoreLOD.Methods;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace TreesIgnoreLOD
{
    [BepInPlugin("com.DestroyedClone.CollisionLODOverride", "Collision LOD Override", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class TreesIgnoreLODPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> cfgLODOverride;
        public static int lodOverrideValue = 0;
        public static bool discoveryMode = false;

        public void Start()
        {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            cfgLODOverride = Config.Bind("", "LOD Override Level", 1, "I'm basing them off how I felt they looked, not anything technical." +
                "\n" +
                "\n0 = Normal" +
                "\n1 = Simpler" +
                "\n2 = Simplest" +
                "\n3 = Leaves are solid as you approach, sometimes trunks are just invisible." +
                "\n-1/Negative numbers = Leaves are almost invisible until you approach them.");

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            On.RoR2.SettingsConVars.MaximumLodConVar.SetString += MaximumLodConVar_SetString;

            if (discoveryMode)
                On.RoR2.SceneDirector.PopulateScene += Discover;
        }

        private void MaximumLodConVar_SetString(On.RoR2.SettingsConVars.MaximumLodConVar.orig_SetString orig, RoR2.ConVar.BaseConVar self, string newValue)
        {
            orig(self, newValue);
            string[] chosenPathSet = GetPathSet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            if (chosenPathSet != null)
            {
                //Chat.AddMessage("Overriding");
                PatchScene(chosenPathSet, cfgLODOverride.Value);
            }
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            string[] chosenPathSet = GetPathSet(scene.name);
            if (chosenPathSet != null)
            {
                PatchScene(chosenPathSet, cfgLODOverride.Value);
            }
        }

        private void Discover(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            PrintSceneCollisions();
        }

        [ConCommand(commandName = "collision_lod_override_preset_modify", flags = ConVarFlags.ExecuteOnServer, helpText = "collision_lod_override_preset_modify {0, 1, 2, 3, -#} - Overrides the collideable LOD currently in the scene for preview, temporary.")]
        private static void ModifyPresetScene(ConCommandArgs args)
        {
            var value = args.GetArgInt(1);
            PatchScene(GetPathSet(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name), value);
        }

        [ConCommand(commandName = "collision_lod_print", flags = ConVarFlags.ExecuteOnServer, helpText = "collision_lod_print {printPath:true/false} - Prints the scenename followed by the amount of collideable LOD groups, uncollideable LOD groups, and the total count." +
            "\nOptionally, include 'true' to print the paths of the LOD objects for easy indexing to author.")]
        private static void DiscoverScene(ConCommandArgs args)
        {
            bool shouldPrint = args.Count > 0 ? args.GetArgBool(0) : false;
            PrintSceneCollisions(shouldPrint);
        }
    }
}