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
        public class SyncSomething : INetMessage
        {
            NetworkInstanceId netId; //CharacterBody.gameObject
            int indicatorType;

            public SyncSomething()
            {
            }

            public SyncSomething(NetworkInstanceId netId, int indicatorType)
            {
                this.netId = netId;
                this.indicatorType = indicatorType;
                number = num;
            }


            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    Debug.Log("SyncSomething: Host ran this. Skip.");
                    return;
                }
                switch (indicatorType)
                {
                    case 0: // Typing Indicator
                        break;
                    case 1: // Tabbed out Indicator
                        break;
                }


                Chat.AddMessage($"Client received SyncSomething. Position received is {position}. Number received is {number}.");
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    Debug.Log("SyncSomething: bodyObject is null.");
                    return;
                }
                Util.PlaySound("somevanillasoundstring", bodyObject);
            }

            // method that will write the variables into the network coming from the caller of the machine
            public void Serialize(NetworkWriter writer)
            {
                // Order Matters
                writer.Write(netId);
            }

            // method that handles how to read the data that was received on clients
            public void Deserialize(NetworkReader reader)
            {
                // Order must match serialize
                netId = reader.ReadNetworkId();
                position = reader.ReadVector3();
                number = reader.ReadInt32();
            }
        }
    }
}
