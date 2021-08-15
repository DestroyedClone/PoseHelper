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
        public static string flavorText = "";
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
                    GameObject.Find("Moon2DropshipZone/States/EscapeComplete").SetActive(true);
                }
            }
        }

        [ConCommand(commandName = "show_endgame_text", flags = ConVarFlags.ExecuteOnServer, helpText = "show_endgame_text {BodyName} {win/fail}")]
        private static void SkipMoonWin(ConCommandArgs args)
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
            if ()


            Console.instance.SubmitCmd(null, "set_scene outro", false);
        }

        public class ApprovedToSkipOutro : MonoBehaviour
        {
        }
    }
}
