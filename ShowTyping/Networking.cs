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
                    //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    //{
                    //    baseToken = "TypingTextMessage: Ran on client. Skipping."
                    //});
                    return;
                }
                //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                //{
                //    baseToken = "TypingTextMessage: Server received message."
                //});
                //NetworkUser networkUser = Util.FindNetworkObject(netId).GetComponent<NetworkUser>();
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    //{
                    //    baseToken = "TypingTextMessage: Null bodyobject, leaving.."
                    //});
                    return;
                }
                var typingText = UnityEngine.Object.Instantiate(ShowTypingPlugin.typingText, bodyObject.transform);
                typingText.transform.position = bodyObject.transform.position + Vector3.up*2f;
                typingText.transform.SetParent(bodyObject.transform);
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
                {
                    /*Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "UnfocusedTextMessage: Ran on client. Skipping."
                    });*/
                    return;
                }
                //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                //{
                //    baseToken = "UnfocusedTextMessage: Server received message."
                //});
                //NetworkUser networkUser = Util.FindNetworkObject(netId).GetComponent<NetworkUser>();
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    //{
                    //    baseToken = "UnfocusedTextMessage: Null bodyobject, leaving.."
                    //});
                    return;
                }
                var typingText = UnityEngine.Object.Instantiate(ShowTypingPlugin.unfocusedText, bodyObject.transform);
                typingText.transform.position = bodyObject.transform.position + Vector3.up * 2f;
                typingText.transform.SetParent(bodyObject.transform);
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
