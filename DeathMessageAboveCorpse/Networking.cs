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
            Vector3 position;

            public DeathQuoteMessageToServer() { }

            public DeathQuoteMessageToServer(Vector3 position)
            {
                this.position = position;
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {

                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "Server: Client ran code."
                    });
                    return;
                }

                /*var typingText = UnityEngine.Object.Instantiate(defaultTextObject);
                typingText.transform.position = position;
                DeathMessageLocator deathMessageLocator = typingText.GetComponent<DeathMessageLocator>();
                deathMessageLocator.quoteIndex = UnityEngine.Random.Range(0, deathMessages.Length);
                NetworkServer.Spawn(typingText);*/

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "Server: Received."
                });

                var quoteIndex = UnityEngine.Random.Range(0, deathMessages.Length);
                    new DeathQuoteMessageToClients(quoteIndex, position).Send(R2API.Networking.NetworkDestination.Clients);
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


        public class DeathQuoteMessageToClients : INetMessage
        {
            int index;
            Vector3 position;

            public DeathQuoteMessageToClients() { }

            public DeathQuoteMessageToClients(int index, Vector3 position)
            {
                this.index = index;
                this.position = position;
            }

            public void OnReceived()
            {
                if (NetworkServer.active && !NetworkClient.active)
                {

                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "Client: Server ran code."
                    });
                    return;
                }

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "Client: Received."
                });

                var typingText = UnityEngine.Object.Instantiate(defaultTextObject);
                typingText.transform.position = position;
                DeathMessageLocator deathMessageLocator = typingText.GetComponent<DeathMessageLocator>();
                deathMessageLocator.quoteIndex = index;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(index);
                writer.Write(position);
            }

            public void Deserialize(NetworkReader reader)
            {
                index = reader.ReadInt32();
                position = reader.ReadVector3();
            }
        }
    }
}
