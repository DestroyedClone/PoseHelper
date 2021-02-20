using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using static R2API.Utils.CommandHelper;
using R2API.Utils;
using RoR2;
using UnityEngine;
using EntityStates;
using UnityEngine.Networking;

namespace LeBuilder
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Console Command")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Empty Arg required")]
    public static class ConsoleCommands
    {
        [ConCommand(commandName = "obj_build", flags = ConVarFlags.ExecuteOnServer, 
            helpText = "obj_build {objectname} {modelName} {opt:materialname} {opt:collisionname}")]
        private static void DeathStateClear(ConCommandArgs args)
        {
            var deathstate = args.senderBody.GetComponent<CharacterDeathBehavior>();
            if (deathstate) deathstate.deathState = new SerializableEntityStateType();
            args.senderMaster.preventGameOver = true;
        }

        [ConCommand(commandName = "mult_hook", flags = ConVarFlags.ExecuteOnServer, helpText = "cc")]
        private static void MULTCheap(ConCommandArgs args)
        {
            On.RoR2.UI.CharacterSelectController.OnNetworkUserLoadoutChanged += CharacterSelectController_OnNetworkUserLoadoutChanged;
            Debug.Log("Added hook!");
        }

        private static void CharacterSelectController_OnNetworkUserLoadoutChanged(On.RoR2.UI.CharacterSelectController.orig_OnNetworkUserLoadoutChanged orig, RoR2.UI.CharacterSelectController self, NetworkUser networkUser)
        {
            orig(self, networkUser);
            var toolbotSurvivorIndex = SurvivorIndex.Toolbot;
            //var toolbotIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(toolbotSurvivorIndex);
            bool showTeaser = true;
            foreach (var display in self.characterDisplayPads)
            {
                if (display.displaySurvivorIndex == toolbotSurvivorIndex)
                {
                    showTeaser = false;
                    break;
                }
            }

            self.gameObject.transform.parent.gameObject.transform.Find("HANDTeaser")?.gameObject.SetActive(showTeaser);
            //if (networkUser.bodyIndexPreference == toolbotIndex)
            {

            }
        }

        [ConCommand(commandName = "changelight", flags = ConVarFlags.ExecuteOnServer, helpText = "changelight {r} {g} {b} {a}")]
        private static void ChangeLight(ConCommandArgs args)
        {
            var light = args.senderBody.gameObject.transform.parent.transform.Find("Directional Light").gameObject.GetComponent<Light>();
            light.color = new Color32((byte)args.GetArgInt(0), (byte)args.GetArgInt(1), (byte)args.GetArgInt(2), (byte)args.GetArgInt(3));
        }

        [ConCommand(commandName = "past", flags = ConVarFlags.ExecuteOnServer,
            helpText = "past {acrid/commando/engineer/captain}")]
        private static void SelectPast(ConCommandArgs args)
        {
            var character = args.GetArgString(0).ToLower();

            switch (character)
            {
                case "acrid":
                    break;
                case "commando":
                    break;
                case "engineer":
                    break;
                case "captain":
                    break;
                default:
                    Debug.Log("No past data found for this name.");
                    break;
            }
        }



    }
}
