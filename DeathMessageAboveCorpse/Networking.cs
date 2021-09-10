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
                    return;
                }

                var typingText = UnityEngine.Object.Instantiate(defaultTextObject);
                typingText.transform.position = position;
                DeathMessageLocator deathMessageLocator = typingText.GetComponent<DeathMessageLocator>();
                deathMessageLocator.quoteIndex = UnityEngine.Random.Range(0, deathMessages.Length);
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


        public class DeathQuoteMessageToClients : INetMessage
        {
            int index;

            public DeathQuoteMessageToClients() { }

            public DeathQuoteMessageToClients(int index)
            {
                this.index = index;
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }

                var typingText = UnityEngine.Object.Instantiate(defaultTextObject);
                typingText.transform.position = position;
                DeathMessageLocator deathMessageLocator = typingText.GetComponent<DeathMessageLocator>();
                deathMessageLocator.quoteIndex = UnityEngine.Random.Range(0, deathMessages.Length);
                NetworkServer.Spawn(typingText);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(index);
            }

            public void Deserialize(NetworkReader reader)
            {
                index = reader.ReadInt32();
            }
        }
    }
}
