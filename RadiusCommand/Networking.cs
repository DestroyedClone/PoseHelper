using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RadiusCommand
{
    public class Networking
    {
        public class RadiusCommandToServer : INetMessage
        {
            private string cubeIndices;
            private int choiceIndex;

            public RadiusCommandToServer()
            {

            }

            public RadiusCommandToServer(string cubeIndices, int choiceIndex)
            {
                this.cubeIndices = cubeIndices;
                this.choiceIndex = choiceIndex;
            }

            public void OnReceived()
            {
                if (!NetworkServer.active)
                {
                    return;
                }

                var CommandCubes = InstanceTracker.GetInstancesList<PickupPickerController>();

                var cut = cubeIndices.Split(',');
                foreach(var sub in cut)
                {
                    var index = int.Parse(sub);
                    var cube = CommandCubes[index];

                    //var network = cube.networkUIPromptController;
                    //network.currentParticipantMaster = self.networkUIPromptController.currentParticipantMaster;
                    cube.HandlePickupSelected(choiceIndex);
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(cubeIndices);
                writer.Write(choiceIndex);
            }

            public void Deserialize(NetworkReader reader)
            {
                reader.ReadString();
                reader.ReadInt32();
            }

        }
    }
}
