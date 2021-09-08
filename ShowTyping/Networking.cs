using System;
using System.Collections.Generic;
using System.Text;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using ShowTyping;
using UnityEngine;
using RoR2;
using static ShowTyping.ShowTypingPlugin;

namespace ShowTyping
{
    public class Networking
    {
        public class TypingTextMessage : INetMessage
        {
            NetworkInstanceId netId;

            public TypingTextMessage() { }

            public TypingTextMessage(NetworkInstanceId netId)
            {
                this.netId = netId;
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    Chat.AddMessage("TypingTextMessage: Client received message, skipping.");
                    return;
                }
                Chat.AddMessage("TypingTextMessage: Server received message");
                //NetworkUser networkUser = Util.FindNetworkObject(netId).GetComponent<NetworkUser>();
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    Chat.AddMessage("TypingTextMessage: bodyObject is null.");
                    return;
                }
                var typingText = UnityEngine.Object.Instantiate(ShowTypingPlugin.typingText, bodyObject.transform);
                NetworkServer.Spawn(typingText);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
            }

            public void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadNetworkId();
            }
        }


        public class UnfocusedTextMessage : INetMessage
        {
            NetworkInstanceId netId;

            public UnfocusedTextMessage() { }

            public UnfocusedTextMessage(NetworkInstanceId netId)
            {
                this.netId = netId;
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                    return;
                //NetworkUser networkUser = Util.FindNetworkObject(netId).GetComponent<NetworkUser>();
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    Debug.Log("TypingTextMessage: bodyObject is null.");
                    return;
                }
                var typingText = UnityEngine.Object.Instantiate(ShowTypingPlugin.unfocusedText, bodyObject.transform);
                NetworkServer.Spawn(typingText);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
            }

            public void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadNetworkId();
            }
        }
    }
}
