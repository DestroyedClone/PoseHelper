using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

using System.Collections.ObjectModel;
using System.Globalization;
using RoR2.ConVar;
using RoR2.Networking;
using Unity;
using UnityEngine.Networking;
using RoR2.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ChatEdit
{
    [BepInPlugin("com.DestroyedClone.ChatEdit", "ChatEdit", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ChatEditPlugin : BaseUnityPlugin
    {
        public static bool showConnectMessage = false;
        public static bool showDisconnectMessage = false;
        //public static bool playNotifSound = false;
        public static bool mergePickupMessages = true;

        public static CharacterBody MostRecentGrabber = null;
        public static Dictionary<string, uint> Tokens_to_Count = new Dictionary<string, uint>();
        public static Dictionary<string, uint> Tokens_to_NewCount = new Dictionary<string, uint>();
        public static int IndexOfLastPickupMessage = 0;

        public void Awake()
        {
            // This mod is DESTRUCTIVE AND WILL NOT WORK WITH OTHER MODS
            // Viewer discretion is advised
            //IL.RoR2.Chat.AddMessage_string += Chat_AddMessage_string;
            //SetupConfig();

            if (!showConnectMessage)
                On.RoR2.Chat.SendPlayerConnectedMessage += Chat_SendPlayerConnectedMessage;
            if (!showDisconnectMessage)
                On.RoR2.Chat.SendPlayerDisconnectedMessage += Chat_SendPlayerDisconnectedMessage;
            //if (!playNotifSound)
                //On.RoR2.Chat.UserChatMessage.OnProcessed += UserChatMessage_OnProcessed;
            //AddPickupMessage
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            //On.RoR2.Chat.PlayerPickupChatMessage.ConstructChatString += PlayerPickupChatMessage_ConstructChatString;
            //On.RoR2.Chat.AddPickupMessage += Chat_AddPickupMessage;
            if (mergePickupMessages)
            {
                On.RoR2.Chat.AddPickupMessage += Chat_AddPickupMessage1;
                On.RoR2.Chat.AddMessage_string += Chat_AddMessage_string1;
            }
        }

        private void Chat_AddMessage_string1(On.RoR2.Chat.orig_AddMessage_string orig, string message)
        {
            var cachedCount = Chat.log.Count;
            //Debug.Log($"Cached Count: {cachedCount}");
            orig(message);
            //Debug.Log($"New Count: {Chat.log.Count}");
            var difference = cachedCount - Chat.log.Count;
            //Debug.Log($"Difference: {difference}");
            if (difference < 0)
            {
                int newIndex = IndexOfLastPickupMessage + difference;
                //Debug.Log($"NewIndex: {newIndex}");
                if (newIndex >= 0)
                {
                    IndexOfLastPickupMessage = newIndex;
                }
            }
        }

        private void Chat_AddPickupMessage1(On.RoR2.Chat.orig_AddPickupMessage orig, CharacterBody body, string pickupToken, Color32 pickupColor, uint pickupQuantity)
        {
            if (!MostRecentGrabber == body)
            {
                orig(body, pickupToken, pickupColor, pickupQuantity);
                ResetGrabber();
                MostRecentGrabber = body;
                return;
            } else
            {
                Chat.log.RemoveAt(IndexOfLastPickupMessage);
                    //if (Chat.log.Count - 1 > 0)
                        //Chat.log.RemoveAt(Chat.log.Count - 1);
                Tokens_to_Count[pickupToken] = pickupQuantity;

                if (!Tokens_to_NewCount.ContainsKey(pickupToken))
                {
                    Tokens_to_NewCount.Add(pickupToken, 0);
                }
                Tokens_to_NewCount[pickupToken]++;
            }
            // ({Util.GenerateColoredString($"+{Tokens_to_NewCount[pickupToken]}", Color.yellow)})
            string message = "";

            foreach (KeyValuePair<string, uint> kvp in Tokens_to_Count)
            {
                if (kvp.Key == pickupToken)
                {
                    message += $"{Util.GenerateColoredString(Language.GetString(kvp.Key), pickupColor)} ({kvp.Value}) ({Util.GenerateColoredString($"+{Tokens_to_NewCount[pickupToken]}", Color.yellow)})";
                } else
                {
                    message += $"{Util.GenerateColoredString(Language.GetString(kvp.Key), Color.grey)} ({kvp.Value})";
                }
                message += " ";
            }

            var subjectFormatChatMessage = new Chat.SubjectFormatChatMessage
            {
                baseToken = "PLAYER_PICKUP",
                paramTokens = new string[] { message, "" },
                subjectAsCharacterBody = body
            };
            Chat.AddMessage(subjectFormatChatMessage);
            IndexOfLastPickupMessage = Mathf.Max(0, Chat.log.Count - 1);
        }

        private string PlayerPickupChatMessage_ConstructChatString(On.RoR2.Chat.PlayerPickupChatMessage.orig_ConstructChatString orig, Chat.PlayerPickupChatMessage self)
        {
            throw new NotImplementedException();
        }

        private void Chat_SendPlayerDisconnectedMessage(On.RoR2.Chat.orig_SendPlayerDisconnectedMessage orig, NetworkUser user)
        {
            return;
        }

        private void Chat_SendPlayerConnectedMessage(On.RoR2.Chat.orig_SendPlayerConnectedMessage orig, NetworkUser user)
        {
            return;
        }

        private void SetupConfig()
        {
            showConnectMessage = Config.Bind("", "Show Connect Message", true, "If true, the message will be shown. (Vanilla: true)").Value;
            showDisconnectMessage = Config.Bind("", "Show Disconnect Message", true, "If true, the message will be shown. (Vanilla: true)").Value;
            mergePickupMessages = Config.Bind("", "Merge Pickp Messages", true, "If true, the most recent person in chat will be cached." +
                "If they pickup more items, then their message will be merged.").Value;
        }

        private void Chat_AddMessage_string(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdsfld<IntConVar>("")
                );
            c.Index += 4;
            c.Emit(OpCodes.Ldloc);
            c.EmitDelegate<Func<HealthComponent, bool>>((hc) =>
            {
                if ((bool)hc.body?.hasCloakBuff)
                {
                    return false;
                }
                return true;
            });
        }

        [ConCommand(commandName = "chat_clear", flags = ConVarFlags.None, helpText = "Clears the chat.")]
        private static void CCClearChat(ConCommandArgs args)
        {
            Chat.Clear();
        }

        [ConCommand(commandName = "chat_reset_grabber", flags = ConVarFlags.None, helpText = "Resets the last grabber.")]
        private static void CCResetGrabber(ConCommandArgs args)
        {
            ResetGrabber();
        }

        private static void ResetGrabber()
        {
            MostRecentGrabber = null;
            Tokens_to_Count.Clear();
            Tokens_to_NewCount.Clear();
        }
    }
}
