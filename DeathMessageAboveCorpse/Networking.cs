using System;
using System.Collections.Generic;
using System.Text;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using static DeathMessageAboveCorpse.DeathMessageAboveCorpsePlugin;

namespace DeathMessageAboveCorpse
{
    public class Networking
    {
        public class DeathQuoteMessageToServer : INetMessage
        {
            NetworkInstanceId netId;
            Vector3 position;

            public DeathQuoteMessageToServer() { }

            public DeathQuoteMessageToServer(NetworkInstanceId netId, Vector3 position)
            {
                this.netId = netId;
                this.position = position;
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "DeathQuoteMessage: Client received message."
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
                var typingText = UnityEngine.Object.Instantiate(DeathMessageAboveCorpsePlugin.defaultTextObject, bodyObject.transform);
                typingText.transform.position = bodyObject.transform.position + Vector3.up*2f;
                typingText.transform.SetParent(bodyObject.transform);
                NetworkServer.Spawn(typingText);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
                writer.Write(position);
            }

            public void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadNetworkId();
                position = reader.ReadVector3();
            }
        }

        public class DeathQuoteMessageToClients : INetMessage
        {
            Vector3 position;

            public DeathQuoteMessageToClients() { }

            public DeathQuoteMessageToClients(Vector3 position)
            {
                this.position = position;
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "DeathQuoteMessage: Server received message."
                });

                var typingText = UnityEngine.Object.Instantiate(defaultTextObject);
                typingText.transform.position = position;
                NetworkServer.Spawn(typingText);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(position);
            }

            public void Deserialize(NetworkReader reader)
            {
                position = reader.ReadVector3();
            }
        }

    }
}
