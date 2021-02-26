using BepInEx;
using R2API.Utils;
using RoR2;
using BepInEx.Configuration;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.ColorUtility;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace LobbyAppearanceImprovements
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "LobbyAppearanceImprovements";
        public const string ModGuid = "com.DestroyedClone.LobbyAppearanceImprovements";
        public static ConfigEntry<string> DefaultColor { get; set; }
        public static ConfigEntry<bool> StopWave { get; set; }
        public static ConfigEntry<bool> PostProcessing { get; set; }
        public static ConfigEntry<bool> HideFade { get; set; }
        public static ConfigEntry<bool> MeshProps { get; set; }

        public void Awake()
        {
            //default new Color32((byte)0.981, (byte)0.356, (byte)0.356, (byte)1.000)
            //250.155, 90.78, 90.78
            DefaultColor = Config.Bind("Lights", "Hex Color", "#fa5a5a", "Change the default color of the light");
            StopWave = Config.Bind("Lights", "Disable FlickerLight", true, "Makes the light not flicker anymore.");
            PostProcessing = Config.Bind("Overlay", "Disable Post Processing", true, "Disables the blurry post processing.");
            HideFade = Config.Bind("Overlay", "Hide Fade", true, "There's a dark fade on the top and bottom, this disables it.");
            MeshProps = Config.Bind("Background", "Hide MeshProps", false, "Hides all the meshprops, giving a unique look.");
            CommandHelper.AddToConsoleWhenReady();

            if (StopWave.Value || MeshProps.Value || PostProcessing.Value)
            {
                On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;
            }

        }

        private void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            if (TryParseHtmlString(DefaultColor.Value, out Color color))
                Helpers.ChangeLobbyLightColor(color);
            if (StopWave.Value)
            {
                GameObject.Find("Directional Light").gameObject.GetComponent<FlickerLight>().enabled = false;
            }
            if (MeshProps.Value)
            {
                GameObject.Find("HANDTeaser").SetActive(false);
                GameObject.Find("MeshProps").SetActive(false);
                GameObject.Find("HumanCrate1Mesh").SetActive(false);
                GameObject.Find("HumanCrate2Mesh").SetActive(false);
                GameObject.Find("HumanCanister1Mesh").SetActive(false);
            }
            if (PostProcessing.Value)
            {
                GameObject.Find("PP").SetActive(false);
            }
            if (HideFade.Value)
            {
                var ui = GameObject.Find("CharacterSelectUI").transform;
                ui.Find("BottomSideFade").gameObject.SetActive(false);
                ui.Find("TopSideFade").gameObject.SetActive(false);
            }
        }
    }

    public static class Commands
    {

        [ConCommand(commandName = "changelight", flags = ConVarFlags.ExecuteOnServer, helpText = "changelight {r} {g} {b} {a} | only works in the lobby")]
        public static void ChangeLight(ConCommandArgs args)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
                Helpers.ChangeLobbyLightColor(new Color32((byte)args.GetArgInt(0), (byte)args.GetArgInt(1), (byte)args.GetArgInt(2), (byte)args.GetArgInt(3)));
        }
    }

    public static class Helpers
    {
        public static void ChangeLobbyLightColor(Color32 color)
        {
            GameObject.Find("Directional Light").gameObject.GetComponent<Light>().color = color;
        }
    }
}
