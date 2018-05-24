using System.Collections;
using System.Collections.Generic;
using LiteNetLib;

namespace Insthync.MMOG
{
    public enum CentralServerPeerType : byte
    {
        LoginServer,
        ChatServer,
        MapSpawnServer,
        MapServer,
    }
}
