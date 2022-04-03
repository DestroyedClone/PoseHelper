using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ChatboxEdit
{
    [BepInPlugin("com.DestroyedClone.ChatboxEdit", "ChatboxEdit", "1.0.1")]
    public class ChatboxEditPlugin : BaseUnityPlugin
    {
        public static GameObject Prefab = LegacyResourcesAPI.Load<GameObject>("prefabs/ChatBox, In Run");
        public static GameObject PrefabLobby = LegacyResourcesAPI.Load<GameObject>("prefabs/ChatBox");

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
    }
}