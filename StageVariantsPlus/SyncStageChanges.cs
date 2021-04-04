using Unity;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace StageVariantsPlus
{
    public class SyncStageChanges : NetworkBehaviour
    {
        [SyncVar]
        uint VariantToEnable = 0;

        [ClientRpc]
        void RpcUpdateStage()
        {

        }
    }
}
