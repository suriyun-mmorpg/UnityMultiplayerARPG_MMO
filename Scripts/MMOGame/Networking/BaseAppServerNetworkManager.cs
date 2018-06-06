using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public abstract class BaseAppServerNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public const float RECONNECT_DELAY = 5f;
        public abstract CentralServerPeerType PeerType { get; }
        [Header("App Server Configs")]
        public string machineAddress = "127.0.0.1";
        public string centralServerAddress = "127.0.0.1";
        public int centralServerPort = 6000;
        public string centralServerConnectKey = "SampleConnectKey";
        private CentralNetworkManager cacheCentralNetworkManager;
        public CentralNetworkManager CacheCentralNetworkManager
        {
            get
            {
                if (cacheCentralNetworkManager == null)
                    cacheCentralNetworkManager = gameObject.AddComponent<CentralNetworkManager>();
                cacheCentralNetworkManager.currentLogLevel = currentLogLevel;
                return cacheCentralNetworkManager;
            }
        }

        protected void OnEnable()
        {
            CacheCentralNetworkManager.onClientConnected += OnCentralServerConnected;
            CacheCentralNetworkManager.onClientDisconnected += OnCentralServerDisconnected;
        }

        protected void OnDisable()
        {
            CacheCentralNetworkManager.onClientConnected -= OnCentralServerConnected;
            CacheCentralNetworkManager.onClientDisconnected -= OnCentralServerDisconnected;
        }

        public override void OnStartServer()
        {
            Debug.Log("[" + PeerType + "] Starting server");
            base.OnStartServer();
            ConnectToCentralServer();
        }

        public void ConnectToCentralServer()
        {
            Debug.Log("[" + PeerType + "] Connecting to Central Server");
            CacheCentralNetworkManager.StartClient(centralServerAddress, centralServerPort, centralServerConnectKey);
        }

        public void DisconnectFromCentralServer()
        {
            Debug.Log("[" + PeerType + "] Disconnecting from Central Server");
            CacheCentralNetworkManager.StopClient();
        }

        public virtual void OnCentralServerConnected(NetPeer netPeer)
        {
            Debug.Log("[" + PeerType + "] Connected to Central Server");
            var peerInfo = new CentralServerPeerInfo();
            peerInfo.peerType = PeerType;
            peerInfo.networkAddress = machineAddress;
            peerInfo.networkPort = networkPort;
            peerInfo.extra = GetExtra();
            CacheCentralNetworkManager.RequestAppServerRegister(peerInfo, OnAppServerRegistered);
        }

        public virtual void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("[" + PeerType + "] Disconnected from Central Server");
            StartCoroutine(CentralServerReconnectRoutine());
        }

        IEnumerator CentralServerReconnectRoutine()
        {
            Debug.Log("[" + PeerType + "] Reconnect to central in " + RECONNECT_DELAY + " seconds");
            yield return new WaitForSeconds(RECONNECT_DELAY);
            ConnectToCentralServer();
        }

        public virtual void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message) { }

        public virtual string GetExtra()
        {
            return string.Empty;
        }
    }
}
