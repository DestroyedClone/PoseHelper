using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace ForcedStageVariation
{
    public class FSVNetworkSync : NetworkBehaviour
    {
        [SyncVar]
        public int rootJungleTreasureChests;
        [SyncVar]
        public int rootJungleTunnelLandmass;
        [SyncVar]
        public int rootJungleHeldRocks;
        [SyncVar]
        public int rootJungleUndergroundShortcut;


    }
}
