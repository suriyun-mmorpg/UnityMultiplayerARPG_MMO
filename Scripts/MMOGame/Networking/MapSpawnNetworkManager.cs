using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class MapSpawnNetworkManager : BaseAppServerNetworkManager
    {
        public override CentralServerPeerType PeerType { get { return CentralServerPeerType.MapSpawnServer; } }

        // This server will connect to central server to receive following data:
        // Database server configuration
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
        }
    }
}
