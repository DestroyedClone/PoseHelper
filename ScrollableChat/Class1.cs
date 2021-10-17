using BepInEx;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using R2API;
using R2API.Utils;
using BepInEx.Configuration;

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

        public static ConfigEntry<bool> ScrollableChat;
        public static ConfigEntry<float> ScrollableChatSensitivity;
        public static ConfigEntry<bool> NoMaskChat;
        public static ConfigEntry<float> FadeDelay;
        public static ConfigEntry<float> FadeDuration;

        public void Start()
        {
            ScrollableChat = Config.Bind("Scrollable Chat","Enable",true,"If true, allows you to scroll the chat by scrolling or clicking and dragging." +
                "\nThe very top of the chat (about 5 messages) will scroll in the opposite direction regardless of choice, weird vanilla thing.");
            ScrollableChatSensitivity = Config.Bind("Scrollable Chat", "Scroll Sensitivity", 5f, "If Scrollable Chat is active, then adjusts the sensitivity of scrolling.");
            NoMaskChat = Config.Bind("", "No Chat Mask", true, "If true, the mask that causes the chat to only reveal the last few messages gets removed, showing all messages." +
                "\nMessages may go off screen, and/or cover the screen, so I'd recommend not setting a high chat fade delay.");
            FadeDelay = Config.Bind("", "Chat Fade Delay", 5f, "");
            FadeDuration = Config.Bind("", "Chat Fade Duration", 5f, "");
            RoR2.Chat.cvChatMaxMessages.value = Config.Bind("", "Max Chat Messages", 30, "").Value;

            var chatbox = Prefab.GetComponent<RoR2.UI.ChatBox>();
            var chatboxLobby = PrefabLobby.GetComponent<RoR2.UI.ChatBox>();

            chatbox.fadeTimer = FadeDelay.Value;
            chatbox.fadeDuration = FadeDuration.Value;

            chatboxLobby.fadeTimer = FadeDelay.Value;
            chatboxLobby.fadeDuration = FadeDuration.Value;


            var scrollRect = chatbox.scrollRect;
            var scrollRectLobby = chatboxLobby.scrollRect;

            if (ScrollableChat.Value)
            {
                scrollRect.enabled = true;
                scrollRect.scrollSensitivity = ScrollableChatSensitivity.Value;
                scrollRect.vertical = true;

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