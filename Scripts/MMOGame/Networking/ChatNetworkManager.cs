using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class ChatNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        // This server will connect to central server to receive following data:
        // Database server configuration
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            // Receiving:
            // - Player Enter Chat Message
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            // Receiving:
            // - Receives Chat Messages
        }
    }
}
