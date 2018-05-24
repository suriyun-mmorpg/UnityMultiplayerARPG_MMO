using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class LoginNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public string publicMachineAddress = "127.0.0.1";
        public string centralServerAddress = "127.0.0.1";
        public int centralServerPort = 6000;
        private CentralNetworkManager cacheCentralNetworkManager;
        public CentralNetworkManager CacheCentralNetworkManager
        {
            get
            {
                if (cacheCentralNetworkManager == null)
                    cacheCentralNetworkManager = gameObject.AddComponent<CentralNetworkManager>();
                return cacheCentralNetworkManager;
            }
        }
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

        public override void OnStartServer()
        {
            base.OnStartServer();
            CacheCentralNetworkManager.onClientConnected = OnCentralClientConnected;
            CacheCentralNetworkManager.onClientDisconnected = OnCentralClientDisconnected;
            CacheCentralNetworkManager.StartClient(centralServerAddress, centralServerPort);
        }

        private void OnCentralClientConnected(NetPeer netPeer)
        {
            // 
        }

        private void OnCentralClientDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {

        }
    }
}
