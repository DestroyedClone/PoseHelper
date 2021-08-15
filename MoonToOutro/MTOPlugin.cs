using BepInEx;
//using R2API.Utils;
//using EnigmaticThunder;
using RoR2;
using UnityEngine;

namespace MoonToOutro
{
    [BepInPlugin("com.DestroyedClone.MoonToOutro", "Immediate Moon To Outro", "1.1.0")]
    //[R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.UI.OutroFlavorTextController.UpdateFlavorText += OutroFlavorTextController_UpdateFlavorText;
        }

        private void OutroFlavorTextController_UpdateFlavorText(On.RoR2.UI.OutroFlavorTextController.orig_UpdateFlavorText orig, RoR2.UI.OutroFlavorTextController self)
        {
            orig(self);

        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (PlayerCharacterMasterController.instances[0].master?.gameObject.GetComponent<ApprovedToSkipOutro>())
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "moon")
                    UnityEngine.Object.FindObjectOfType<EscapeSequenceController>().CompleteEscapeSequence();
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "moon2")
                {

                }
            }
        }

        [ConCommand(commandName = "skipmoon_old", flags = ConVarFlags.ExecuteOnServer, helpText = "Immediately completes Commencement to properly head to the outro quickly. Must be used mid-run.")]
        private static void SkipMoonOld(ConCommandArgs args)
        {
            args.senderMasterObject.AddComponent<ApprovedToSkipOutro>();
            RoR2.Console.instance.SubmitCmd(args.sender, "next_scene moon", false);
        }

        [ConCommand(commandName = "skipmoon_win", flags = ConVarFlags.ExecuteOnServer, helpText = "Beats the game with a win.")]
        private static void SkipMoonWin(ConCommandArgs args)
        {
            args.senderMasterObject.AddComponent<ApprovedToSkipOutro>().gameEndType = GameEndType.Win;
            RoR2.Console.instance.SubmitCmd(args.sender, "next_scene moon2", false);
        }

        [ConCommand(commandName = "skipmoon_fail", flags = ConVarFlags.ExecuteOnServer, helpText = "Ends the game with a fail.")]
        private static void SkipMoonFail(ConCommandArgs args)
        {
            args.senderMasterObject.AddComponent<ApprovedToSkipOutro>().gameEndType = GameEndType.Fail;
            RoR2.Console.instance.SubmitCmd(args.sender, "next_scene moon2", false);
        }

        public class ApprovedToSkipOutro : MonoBehaviour
        {
            public GameEndType gameEndType = GameEndType.Win;
        }

        public enum GameEndType
        {
            Win,
            Fail
        }
    }
}
