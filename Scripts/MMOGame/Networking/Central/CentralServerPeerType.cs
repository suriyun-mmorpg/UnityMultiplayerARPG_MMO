using System.Collections;
using System.Collections.Generic;
using LiteNetLib;

namespace MultiplayerARPG.MMO
{
    public enum CentralServerPeerType : byte
    {
        MapSpawnServer,
        MapServer,
        Chat,
    }
}
