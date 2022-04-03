using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using R2API;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.ObjectModel;
using TMPro;
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ChatboxEdit
{
    [BepInPlugin("com.DestroyedClone.ChatboxEdit", "ChatboxEdit", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ChatboxEditPlugin : BaseUnityPlugin
    {
        public static GameObject Prefab = Resources.Load<GameObject>("prefabs/ChatBox, In Run");
        public static GameObject PrefabLobby = Resources.Load<GameObject>("prefabs/ChatBox");
        public static GameObject PrefabPickup;

        public static ConfigEntry<bool> ScrollableChat;
        public static ConfigEntry<float> ScrollableChatSensitivity;
        public static ConfigEntry<float> FadeDelay;
        public static ConfigEntry<float> FadeDuration;

        public static ConfigEntry<ChatExpandMode> cfgChatExpandeMode;

        public enum ChatExpandMode
        {
            Disabled,
            Expanded,
            Unlimited
        }

        public void Start()
        {
            Hooks();
            ScrollableChat = Config.Bind("Scrollable Chat", "Enable", true, "If true, allows you to scroll the chat by scrolling or clicking and dragging." +
                "\nThe very top of the chat (about 5 messages) will scroll in the opposite direction regardless of choice, weird vanilla thing.");
            ScrollableChatSensitivity = Config.Bind("Scrollable Chat", "Scroll Sensitivity", 5f, "If Scrollable Chat is active, then adjusts the sensitivity of scrolling.");
            FadeDelay = Config.Bind("", "Chat Fade Delay", 5f, "");
            FadeDuration = Config.Bind("", "Chat Fade Duration", 5f, "");
            RoR2.Chat.cvChatMaxMessages.value = Config.Bind("", "Max Chat Messages", 30, "").Value;

            cfgChatExpandeMode = Config.Bind("", "Chat Expand Mode", ChatExpandMode.Disabled, "0. Disabled." +
                "\nExpanded = While the input is open, your visible chat increases by about 5 lines." +
                "\nUnlimited = All messages can show up on screen, can cover your screen.");

            var chatbox = Prefab.GetComponent<RoR2.UI.ChatBox>();
            var chatboxLobby = PrefabLobby.GetComponent<RoR2.UI.ChatBox>();

            chatbox.fadeTimer = FadeDelay.Value;
            chatbox.fadeDuration = FadeDuration.Value;

            chatboxLobby.fadeTimer = FadeDelay.Value;
            chatboxLobby.fadeDuration = FadeDuration.Value;

            switch (cfgChatExpandeMode.Value)
            {
                case ChatExpandMode.Expanded:
                    chatbox.allowExpandedChatbox = true;
                    break;

                case ChatExpandMode.Unlimited:
                    Prefab.transform.Find("StandardRect/Scroll View/Viewport/MessageArea/Text Area").GetComponent<RectMask2D>().enabled = false;
                    Prefab.transform.Find("StandardRect/Scroll View/Viewport").GetComponent<Mask>().enabled = false;
                    Prefab.transform.Find("StandardRect/Scroll View/Viewport").GetComponent<Image>().enabled = false;
                    break;
            }

            if (ScrollableChat.Value)
            {
                var scrollRect = chatbox.scrollRect;
                scrollRect.enabled = true;
                scrollRect.scrollSensitivity = ScrollableChatSensitivity.Value;
                scrollRect.vertical = true;

                var scrollRectLobby = chatboxLobby.scrollRect;
                scrollRectLobby.enabled = true;
                scrollRectLobby.scrollSensitivity = ScrollableChatSensitivity.Value;
                scrollRectLobby.vertical = true;
            }

            /*scrollRect.transform.Find("Viewport/MessageArea/Text Area/MessageArea Input Carat").GetComponent<TMPro.TMP_SelectionCaret>().color = new Color32()
            {
                a = 125
            };*/
        }



        public void Hooks()
        {
            On.RoR2.Chat.AddPickupMessage += Chat_AddPickupMessage;
            On.RoR2.UI.ChatBox.BuildChat += ChatBox_BuildChat;
            SetupAltChatbox();
            On.RoR2.UI.ChatBox.Awake += ChatBox_Awake;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void ChatBox_Awake(On.RoR2.UI.ChatBox.orig_Awake orig, ChatBox self)
        {
            var a = UnityEngine.Object.Instantiate(PrefabPickup, self.transform.parent);
            a.transform.localPosition += Vector3.up * 100f;
            self.fadeWait = 9999999999f;
            self.gameObject.SetActive(false);
            //orig(self);
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            RoR2.Console.instance.SubmitCmd(null, "no_enemies", true);
        }

        private void ChatBox_BuildChat(On.RoR2.UI.ChatBox.orig_BuildChat orig, ChatBox self)
        {
            orig(self);
            if (true == false)
            {
                ReadOnlyCollection<string> readOnlyLog = Chat.readOnlyLog;
                string[] array = new string[readOnlyLog.Count];
                readOnlyLog.CopyTo(array, 0);
                self.messagesText.text = string.Join("\n", array);
                self.RebuildChatRects();
            }

            if (MultipleChatBoxHandler.instance)
            {
                var instance = MultipleChatBoxHandler.instance;
                ReadOnlyCollection<string> readOnlyLog = ChatPickup.readOnlyLog;
                string[] array = new string[readOnlyLog.Count];
                readOnlyLog.CopyTo(array, 0);

                var cock = UnityEngine.Object.FindObjectOfType<ChatBoxPickup>();

                cock.messagesText.text = string.Join("\n", array);
                cock.RebuildChatRects();
            }
        }

        private void Chat_AddPickupMessage(On.RoR2.Chat.orig_AddPickupMessage orig, CharacterBody body, string pickupToken, Color32 pickupColor, uint pickupQuantity)
        {
            orig(body, pickupToken, pickupColor, pickupQuantity);

            var cock = UnityEngine.Object.FindObjectOfType<ChatBoxPickup>();
            ChatPickup.AddPickupMessage(body, pickupToken, pickupColor, pickupQuantity);
        }

        public void SetupAltChatbox()
        {
            //PrefabGeneric = PrefabAPI.InstantiateClone(Prefab, "ChatBoxGeneric", false);
            //var chatBox = PrefabGeneric.GetComponent<RoR2.UI.ChatBox>();
            //var copy = UnityEngine.Object.Instantiate(chatBox.messagesText, chatBox.messagesText.transform.parent);
            var chatBox = Prefab.GetComponent<ChatBox>();

            var handler = Prefab.AddComponent<MultipleChatBoxHandler>();
            //handler.chatBox = chatBox;
            //handler.mainChatBox = Prefab;
            //var pickupMessagesTextObject = UnityEngine.Object.Instantiate(chatBox.messagesText.transform.gameObject, chatBox.messagesText.transform.parent);
            //handler.pickupChatBox = pickupMessagesTextObject;

            PrefabPickup = PrefabAPI.InstantiateClone(Prefab, "ChatboxPickup, In Run", false);
            var old = PrefabPickup.GetComponent<ChatBox>();
            var chatBoxPickup = PrefabPickup.AddComponent<ChatBoxPickup>();
            chatBoxPickup.allowExpandedChatbox = old.allowExpandedChatbox;
            chatBoxPickup.deactivateInputFieldIfInactive = old.deactivateInputFieldIfInactive;
            chatBoxPickup.deselectAfterSubmitChat = old.deselectAfterSubmitChat;
            chatBoxPickup.expandedChatboxRect = old.expandedChatboxRect;
            chatBoxPickup.fadeDuration = old.fadeDuration;
            chatBoxPickup.fadeWait = old.fadeWait;
            chatBoxPickup.gameplayHiddenGraphics = old.gameplayHiddenGraphics;
            chatBoxPickup.inputField = old.inputField;
            chatBoxPickup.messagesText = old.messagesText;
            chatBoxPickup.scrollRect = old.scrollRect;
            chatBoxPickup.sendButton = old.sendButton;
            chatBoxPickup.standardChatboxRect = old.standardChatboxRect;
            UnityEngine.Object.Destroy(old);
            PrefabPickup.transform.Find("PermanentBG").gameObject.SetActive(false);
        }

        public class MultipleChatBoxHandler : MonoBehaviour
        {
            public ReadOnlyCollection<string> currentChat;

            public GameObject mainChatBox;
            public ChatBox chatBox;

            public GameObject pickupChatBox;

            public static MultipleChatBoxHandler instance;

            public void OnEnable()
            {
                instance = this;
            }

            public void OnDisable()
            {
                instance = null;
            }

            public void Start()
            {
                currentChat = Chat.readOnlyLog;
            }
        }

    }
}