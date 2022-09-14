using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ShareYourMoney
{
    public class Networking
    {
        public static void SendToChat(string text)
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = text
            });
        }


        public class DoshDropMessageToServer : INetMessage
        {
            NetworkInstanceId netId;
            uint moneyDropped;

            public DoshDropMessageToServer()
            {
            }

            public DoshDropMessageToServer(NetworkInstanceId NetId, uint MoneyDropped)
            {
                this.netId = NetId;
                this.moneyDropped = MoneyDropped;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(netId);
                writer.Write(moneyDropped);
            }

            public void Deserialize(NetworkReader reader)
            {
                reader.ReadNetworkId();
                reader.ReadUInt32();
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    SendToChat("DoshDropMessageToServer: NetworkMessage sent to server was sent to a client!");
                    return;
                }
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage {baseToken = $"netID {netId}, dosh: {moneyDropped}" });
                GameObject bodyObject = Util.FindNetworkObject(netId);
                if (!bodyObject)
                {
                    SendToChat("DoshDropMessageToServer: bodyObject is null.");
                    return;
                }
                CharacterBody characterBody = bodyObject.GetComponent<CharacterBody>();
                if (!characterBody)
                {
                    SendToChat("DoshDropMessageToServer: characterBody is null.");
                    return;
                }
                if (!characterBody.master)
                {
                    SendToChat("DoshDropMessageToServer: characterMaster is null.");
                    return;
                }
                Main.ReleaseMoney(characterBody.master, moneyDropped);
            }
        }
    }
}