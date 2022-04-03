using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ChatboxEdit
{
    public static class ChatPickup
    {
        public static ReadOnlyCollection<string> readOnlyLog
        {
            get
            {
                return ChatPickup._readOnlyLog;
            }
        }

        public static event Action onChatChanged;

        public static void AddMessage(string message)
        {
            int num = Mathf.Max(ChatPickup.cvChatMaxMessages.value, 1);
            while (ChatPickup.log.Count > num)
            {
                ChatPickup.log.RemoveAt(0);
            }
            ChatPickup.log.Add(message);
            if (ChatPickup.onChatChanged != null)
            {
                ChatPickup.onChatChanged();
            }
            Debug.Log(message);
        }

        public static void Clear()
        {
            ChatPickup.log.Clear();
            Action action = ChatPickup.onChatChanged;
            if (action == null)
            {
                return;
            }
            action();
        }

        public static void AddPickupMessage(CharacterBody body, string pickupToken, Color32 pickupColor, uint pickupQuantity)
        {
            ChatPickup.AddMessage(new Chat.PlayerPickupChatMessage
            {
                subjectAsCharacterBody = body,
                baseToken = "PLAYER_PICKUP",
                pickupToken = pickupToken,
                pickupColor = pickupColor,
                pickupQuantity = pickupQuantity
            });
        }

        private static void AddMessage(ChatMessageBase message)
        {
            string text = message.ConstructChatString();
            if (text != null)
            {
                ChatPickup.AddMessage(text);
                message.OnProcessed();
            }
        }

        private static List<string> log = new List<string>();

        private static ReadOnlyCollection<string> _readOnlyLog = ChatPickup.log.AsReadOnly();

        private static IntConVar cvChatMaxMessages = new IntConVar("chat_max_pickup_messages", ConVarFlags.None, "30", "Maximum number of pickup messages to store.");
    }
}