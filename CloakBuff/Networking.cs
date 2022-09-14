using System;
using System.Collections.Generic;
using System.Text;
using R2API.Networking.Interfaces;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;

namespace CloakBuff
{
    public class Networking
    {
        public class SendToClientsToDeleteIndicator : INetMessage
        {
            public int index;

            public SendToClientsToDeleteIndicator()
            {
            }

            public SendToClientsToDeleteIndicator(int Index)
            {
                index = Index;
            }

            public void Deserialize(NetworkReader reader)
            {
                reader.ReadInt32();
            }

            public void OnReceived()
            {
                var instanceTracker = InstanceTracker.GetInstancesList<CloakBuffPlugin.KillPingerIfCloaked>();
                if (instanceTracker != null)
                {
                    if (instanceTracker[index])
                    {
                        instanceTracker[index].shouldDestroy = true;
                        Chat.AddMessage("Recevied message.");
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(index);
            }
        }
    }
}