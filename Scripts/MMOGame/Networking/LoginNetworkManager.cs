using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class LoginNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        // This server will connect to central server to receive following data:
        // Map servers addresses, Database server configuration
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            // Receiving:
            // - Player Login Request, Then Reponse Login Status
            // - Player Character List Request,
            // - Player Create Character Request, 
            // - Player Delete Character Request
            // - Player Start Game Request
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            // Receiving:
            // - Login Status
            // - Character List Or Error Status
            // - Create Character Status
            // - Delete Character Status
            // - Start Game Status
            // - Another Player Try To Login Messages
        }
    }
}
