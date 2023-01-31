using BepInEx;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.VoidRaidCrab;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
//using ThreeEyedGames;
using ThreeEyedGames;
using R2API.Networking.Interfaces;
using RoR2.Networking;
using UnityEngine.UIElements;

namespace SprayMod
{
    public class Networking
    {
        public class ServerRequestingClientInfo : INetMessage
        {

            public ServerRequestingClientInfo()
            {

            }

            public ServerRequestingClientInfo(NetworkInstanceId netId)
            {
            }

            public void Deserialize(NetworkReader reader)
            {
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    Debug.Log($"{nameof(ServerRequestingClientInfo)}: Host ran this. Skip.");
                    return;
                }
                //I'm a client(s) :D
                //server wants my url
                //okey
                //here you go
                NetworkInstanceId netId = LocalUserManager.GetFirstLocalUser().cachedMaster.networkIdentity.netId;
                new ClientSendingSelfInfoToServer(SprayModMain.cfgMySprayURL.Value, netId).Send(R2API.Networking.NetworkDestination.Server);
            }

            public void Serialize(NetworkWriter writer)
            {
            }
        }

        public class ClientSendingSelfInfoToServer : INetMessage
        {
            string imageURL;
            NetworkInstanceId clientId;

            public ClientSendingSelfInfoToServer() { }

            public ClientSendingSelfInfoToServer(string imageURL, NetworkInstanceId netId)
            {
                this.imageURL = imageURL;
                this.clientId = netId;
            }

            public void Deserialize(NetworkReader reader)
            {
                reader.ReadString();
                reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    Debug.Log($"{nameof(ClientSendingSelfInfoToServer)}: Client ran this. Skip.");
                    return;
                }
                //im the server
                //i will now give everyone a high five under the table
                //after sanitizing

                new ServerSharingSyncInfo(clientId, imageURL).Send(NetworkDestination.Clients);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(imageURL);
                writer.Write(clientId);
            }
        }

        public class ServerSharingSyncInfo : INetMessage
        {
            public NetworkInstanceId clientInstanceId;
            public string clientSprayURL;

            public ServerSharingSyncInfo() { }

            public ServerSharingSyncInfo(NetworkInstanceId clientInstanceId, string clientSprayURL)
            {
                this.clientInstanceId = clientInstanceId;
                this.clientSprayURL = clientSprayURL;
            }

            public void Deserialize(NetworkReader reader)
            {
                reader.ReadNetworkId();
                reader.ReadString();
            }

            public void OnReceived()
            {
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(clientInstanceId);
                writer.Write(clientSprayURL);
            }
        }
    }
}
