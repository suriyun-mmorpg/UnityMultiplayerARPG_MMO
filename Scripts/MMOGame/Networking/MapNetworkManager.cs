using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class MapNetworkManager : BaseRpgNetworkManager
    {
        // This server will connect to central server to receive following data:
        // Login server address, Chat server address, Database server configuration
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
