using System;
using System.Collections.Generic;
using System.Text;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using ShowTyping;
using UnityEngine;
using RoR2;
using static DeathMessageAboveCorpse.DeathMessageAboveCorpsePlugin;

namespace DeathMessageAboveCorpse
{
    public class Networking
    {
        public class DeathQuoteMessage : INetMessage
        {
            NetworkInstanceId netId;

            public DeathQuoteMessage() { }

            public DeathQuoteMessage(NetworkInstanceId netId)
            {
                this.netId = netId;
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "DeathQuoteMessage: Ran on client. Skipping."
                    });
                    return;
                }
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "DeathQuoteMessage: Server received message."
                });
                //NetworkUser networkUser = Util.FindNetworkObject(netId).GetComponent<NetworkUser>();
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "DeathQuoteMessage: Null bodyobject, leaving.."
                    });
                    return;
                }
                var typingText = UnityEngine.Object.Instantiate(ShowMultiplayerStatusIndicatorsPlugin.typingText, bodyObject.transform);
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
    }
}
