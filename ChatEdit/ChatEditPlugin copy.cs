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
        public static bool cfgShowConnectMessage = false;
        public static bool cfgShowDisconnectMessage = false;
        //public static bool playNotifSound = false;
        public static bool cfgMergePickupMessages = true;
        //public static string cfgCensorList;
        public static bool cfgPrefixDeath = true;

        public static CharacterBody MostRecentGrabber = null;
        public static string LastPickupToken = "";
        public static uint LastPickupCount = 0;

        public static int IndexOfLastPickupMessage = 0;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            SetupConfig();

            if (!cfgShowConnectMessage)
                On.RoR2.Chat.SendPlayerConnectedMessage += Chat_SendPlayerConnectedMessage;
            if (!cfgShowDisconnectMessage)
                On.RoR2.Chat.SendPlayerDisconnectedMessage += Chat_SendPlayerDisconnectedMessage;
            //if (!playNotifSound)
            //On.RoR2.Chat.UserChatMessage.OnProcessed += UserChatMessage_OnProcessed;
            //AddPickupMessage
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            //On.RoR2.Chat.PlayerPickupChatMessage.ConstructChatString += PlayerPickupChatMessage_ConstructChatString;
            //On.RoR2.Chat.AddPickupMessage += Chat_AddPickupMessage;
            if (cfgMergePickupMessages)
            {
                On.RoR2.Chat.AddPickupMessage += Chat_AddPickupMessage1;
                On.RoR2.Chat.AddMessage_string += Chat_AddMessage_string1;
            }

            if (cfgPrefixDeath)
            {
                //On.RoR2.ChatMessageBase.GetObjectName += ChatMessageBase_GetObjectName;
                On.RoR2.Chat.UserChatMessage.ConstructChatString += UserChatMessage_ConstructChatString;
                On.RoR2.Chat.CCSay += Chat_CCSay;
            }

            On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
        }

        private void Chat_CCSay(On.RoR2.Chat.orig_CCSay orig, ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            if (args.sender)
            {
                Chat.SendBroadcastChat(new Chat.UserChatMessage
                {
                    sender = args.sender.gameObject,
                    text = args[0]
                });
            }
            orig(args);
        }

        private string UserChatMessage_ConstructChatString(On.RoR2.Chat.UserChatMessage.orig_ConstructChatString orig, Chat.UserChatMessage self)
        {
            var text = orig(self);
            if (self.sender)
            {
                NetworkUser component = self.sender.GetComponent<NetworkUser>();
                if (component)
                {
                    if (component.master && component.master.IsDeadAndOutOfLivesServer())
                    {
                        if (NetworkServer.active)
                            Chat.AddMessage("Server");
                        else
                            Chat.AddMessage("Client");
                        return string.Format(CultureInfo.InvariantCulture, "<color=#e5eefc>{0}: {1}</color>", "<sprite name=\"Skull\" tint=1>" + Util.EscapeRichTextForTextMeshPro(component.userName), Util.EscapeRichTextForTextMeshPro(self.text));
                    }
                }
            }
            return text;
        }

        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (NetworkServer.active)
            {
                self.lostBodyToDeath = true;
                self.deathFootPosition = body.footPosition;
                RoR2.CharacterAI.BaseAI[] array = self.aiComponents;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].OnBodyDeath(body);
                }
                if (self.playerCharacterMasterController)
                {
                    self.playerCharacterMasterController.OnBodyDeath();
                }
                if (self.inventory.GetItemCount(RoR2Content.Items.ExtraLife) > 0)
                {
                    self.inventory.RemoveItem(RoR2Content.Items.ExtraLife, 1);
                    base.Invoke("RespawnExtraLife", 2f);
                    base.Invoke("PlayExtraLifeSFX", 1f);
                }
                else
                {
                    if (self.destroyOnBodyDeath)
                    {
                        UnityEngine.Object.Destroy(base.gameObject, 1f);
                    }
                    self.preventGameOver = true;
                    self.preventRespawnUntilNextStageServer = true;
                }
                self.ResetLifeStopwatch();
            }
            UnityEngine.Events.UnityEvent unityEvent = self.onBodyDeath;
            if (unityEvent == null)
            {
                return;
            }
            unityEvent.Invoke();
        }

        private string ChatMessageBase_GetObjectName(On.RoR2.ChatMessageBase.orig_GetObjectName orig, ChatMessageBase self, GameObject namedObject)
        {
            var original = orig(self, namedObject);
            if (namedObject)
            {
                NetworkUser networkUser = namedObject.GetComponent<NetworkUser>();
                if (!networkUser)
                {
                    networkUser = Util.LookUpBodyNetworkUser(namedObject);
                }
                if (networkUser)
                {
                    original = "<sprite name=\"Skull\" tint=1>" + original;
                    //result = Util.EscapeRichTextForTextMeshPro(networkUser.userName);
                }
            }

            return original;
        }

        private void Chat_SendBroadcastChat_ChatMessageBase_int(On.RoR2.Chat.orig_SendBroadcastChat_ChatMessageBase_int orig, ChatMessageBase message, int channelIndex)
        {
            orig(message, channelIndex);
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
            if (MostRecentGrabber != body)
            {
                orig(body, pickupToken, pickupColor, pickupQuantity);
                ResetGrabber(body);
                return;
            }
            else
            {
                if (LastPickupToken != pickupToken) //this if statement is kinda hard to deal with
                {
                    LastPickupToken = pickupToken;
                    LastPickupCount = 0;
                    orig(body, pickupToken, pickupColor, pickupQuantity);
                    return;
                }
                else
                {
                    Chat.log.RemoveAt(IndexOfLastPickupMessage);
                    LastPickupCount++;
                }

            }
            // ({Util.GenerateColoredString($"+{Tokens_to_NewCount[pickupToken]}", Color.yellow)})
            string message = $"{Util.GenerateColoredString(Language.GetString(LastPickupToken), pickupColor)}({pickupQuantity}) ({Util.GenerateColoredString($"+{LastPickupCount}", Color.yellow)})";

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
            cfgShowConnectMessage = Config.Bind("", "Show Connect Message", true, "If true, the message will be shown. (Vanilla: true)").Value;
            cfgShowDisconnectMessage = Config.Bind("", "Show Disconnect Message", true, "If true, the message will be shown. (Vanilla: true)").Value;
            cfgMergePickupMessages = Config.Bind("", "Merge Pickp Messages", true, "If true, the most recent person in chat will be cached." +
                "If they pickup more items, then their message will be merged.").Value;
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

        private static void ResetGrabber(CharacterBody replacement = null)
        {
            MostRecentGrabber = replacement;
            LastPickupToken = "";
            LastPickupCount = 0;
        }
    }
}