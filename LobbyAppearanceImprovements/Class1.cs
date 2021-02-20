using BepInEx;
using R2API.Utils;
using RoR2;
using BepInEx.Configuration;
using UnityEngine;
using System.Security;
using System.Security.Permissions;

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
        public static ConfigEntry<Color32> DefaultColor { get; set; }
        public static ConfigEntry<bool> StopWave { get; set; }
        public static ConfigEntry<bool> MeshProps { get; set; }

        public void Awake()
        {
            DefaultColor = Config.Bind("Lights", "Color", new Color32((byte)0.981, (byte)0.356, (byte)0.356, (byte)1.000), "Change the default color of the light");
            StopWave = Config.Bind("Lights", "Remove FlickerLight", true, "Makes the light not flicker anymore.");
            MeshProps = Config.Bind("Lobby", "Hide MeshProps", true, "Hides all the meshprops, giving a unique look.");
            CommandHelper.AddToConsoleWhenReady();

            if (StopWave.Value || MeshProps.Value)
            {
                On.RoR2.SceneDirector.Start += SceneDirector_Start;
            }

        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
            {
                ChangeLobbyLightColor(DefaultColor.Value);
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
            }
        }

        [ConCommand(commandName = "changelight", flags = ConVarFlags.ExecuteOnServer, helpText = "changelight {r} {g} {b} {a} | only works in the lobby")]
        public void ChangeLight(ConCommandArgs args)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
                ChangeLobbyLightColor(new Color32((byte)args.GetArgInt(0), (byte)args.GetArgInt(1), (byte)args.GetArgInt(2), (byte)args.GetArgInt(3)));
        }

        public void ChangeLobbyLightColor(Color32 color)
        {
            GameObject.Find("Directional Light").gameObject.GetComponent<Light>().color = color;
        }
    }
}
