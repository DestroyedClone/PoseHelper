using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security.Permissions;
using UnityEngine;
using R2API.Utils;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MoonToOutro
{
    [BepInPlugin("com.DestroyedClone.SkipToOutroText", "Skip To Outro Text", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> JustFlavorText { get; set; }
        public static ConfigEntry<float> JustFlavorTextTime { get; set; }
        //public static ConfigEntry<bool> DisableVoteController { get; set; }

        public static string flavorText = "";

        public void Awake()
        {
            JustFlavorText = Config.Bind("", "Immediately Show Flavor Text", true, "Skips the cutscene ahead to a specified duration to the outro text.");
            JustFlavorTextTime = Config.Bind("", "Immediately Show Flavor Text Time", 45f, "Duration of the PlayableDirector to skip ahead for the other option.");
            //DisableVoteController = Config.Bind("", "Disable Vote Controller", true, "Disables the vote controller, because it can steal focus from the console when hitting SPACE");

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            On.RoR2.UI.OutroFlavorTextController.UpdateFlavorText += OutroFlavorTextController_UpdateFlavorText;
            if (JustFlavorText.Value)
                On.RoR2.OutroCutsceneController.OnEnable += SpeedUp;

            //failsafe for when the user leaves before the flavortext is reset
            On.RoR2.UI.MainMenu.MainMenuController.Start += ResetFlavortext;
        }

        private void ResetFlavortext(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            flavorText = "";
        }

        private void SpeedUp(On.RoR2.OutroCutsceneController.orig_OnEnable orig, OutroCutsceneController self)
        {
            orig(self);

            if (flavorText != "")
            {
                self.playableDirector.time = JustFlavorTextTime.Value;
            }
        }

        private void OutroFlavorTextController_UpdateFlavorText(On.RoR2.UI.OutroFlavorTextController.orig_UpdateFlavorText orig, RoR2.UI.OutroFlavorTextController self)
        {
            orig(self);
            if (flavorText != "")
            {
                if (self.languageTextMeshController)
                {
                    self.languageTextMeshController.token = flavorText;
                }
                flavorText = "";
            }
        }

        [ConCommand(commandName = "show_outro", flags = ConVarFlags.ExecuteOnServer, helpText = "show_outro {BodyName} {win/fail} - Shows the outro with the ending quote. The second argument defaults to win." +
            "\nAlternatively: \"show_outro custom {string}\" will display a custom string instead.")]
        private static void ShowEndgameText(ConCommandArgs args)
        {
            if (args.GetArgString(0).ToLower() == "custom")
            {
                flavorText = args.GetArgString(1);
            }
            else
            {
                var bodyIndex = BodyCatalog.FindBodyIndexCaseInsensitive(args.GetArgString(0));
                if (bodyIndex < 0)
                {
                    Debug.Log("Couldn't find body index!");
                    return;
                }
                var survivorIndex = SurvivorCatalog.GetSurvivorIndexFromBodyIndex(bodyIndex);
                if (survivorIndex < 0)
                {
                    Debug.Log("Couldn't find survivor index!");
                    return;
                }
                SurvivorDef survivorDef = SurvivorCatalog.GetSurvivorDef(survivorIndex);
                if (!survivorDef)
                {
                    Debug.Log("SurvivorDef not found!");
                    return;
                }

                bool isWinQuote = true;
                if (args.Count == 2)
                {
                    if (args.GetArgString(1).ToLower() == "fail")
                    {
                        isWinQuote = false;
                    }
                }
                flavorText = GetOutroText(survivorDef, isWinQuote);
            }
            Debug.Log("Outro Text: " + flavorText);
            Debug.Log(Language.GetString(flavorText));
            RoR2.Console.instance.SubmitCmd(null, "set_scene outro", false);
        }

        public static string GetOutroText(SurvivorDef survivorDef, bool isWinQuote)
        {
            return isWinQuote ? survivorDef.outroFlavorToken : survivorDef.mainEndingEscapeFailureFlavorToken;
        }
    }
}