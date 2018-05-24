using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace Insthync.MMOG
{
    public abstract class BaseAppServerNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public abstract CentralServerPeerType PeerType { get; }
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

        protected override void Awake()
        {
            base.Awake();
            // Read arguments
        }

        public void ConnectToCentralServer()
        {
            CacheCentralNetworkManager.onClientConnected = OnCentralServerConnected;
            CacheCentralNetworkManager.onClientDisconnected = OnCentralServerDisconnected;
            CacheCentralNetworkManager.StartClient(centralServerAddress, centralServerPort);
        }

        public void DisconnectFromCentralServer()
        {
            CacheCentralNetworkManager.StopClient();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            ConnectToCentralServer();
        }

        public virtual void OnCentralServerConnected(NetPeer netPeer) { }
        public virtual void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo) { }
    }
}
