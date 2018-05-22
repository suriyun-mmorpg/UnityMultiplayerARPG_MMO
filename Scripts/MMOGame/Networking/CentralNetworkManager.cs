using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            // Receiving:
            // - Login Server Request To Store Information (Machine Address/Port)
            // - Chat Server Request To Store Information (Machine Address/Port)
            // - Map Servers Request To Store Information (Machine Address/Port)
            // - Login Server Request For Map Server Information
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            // Receiving:
            // - Chat Server Information
            // - Map Server Information
        }
    }
}
