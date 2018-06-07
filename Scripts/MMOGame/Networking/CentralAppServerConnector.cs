using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class CentralAppServerConnector : MonoBehaviour
    {
        public const float RECONNECT_DELAY = 5f;

        [Header("Connection")]
        public string machineAddress = "127.0.0.1";
        public string centralServerAddress = "127.0.0.1";
        public int centralServerPort = 6000;
        public string centralServerConnectKey = "SampleConnectKey";
        public bool RegisteredToCentralServer { get; private set; }
        private CentralServerPeerType peerType;
        private int networkPort;
        private string extra;

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
        public System.Action<AckResponseCode, BaseAckMessage> onAppServerRegistered;

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

        public void OnStartServer(CentralServerPeerType peerType, int networkPort, string extra)
        {
            this.peerType = peerType;
            this.networkPort = networkPort;
            this.extra = extra;
            Debug.Log("[" + peerType + "] Starting server");
            ConnectToCentralServer();
        }

        public void OnStopServer()
        {
            Debug.Log("[" + peerType + "] Stopping server");
            DisconnectFromCentralServer();
        }

        public void ConnectToCentralServer()
        {
            Debug.Log("[" + peerType + "] Connecting to Central Server");
            CacheCentralNetworkManager.StartClient(centralServerAddress, centralServerPort, centralServerConnectKey);
        }

        public void DisconnectFromCentralServer()
        {
            Debug.Log("[" + peerType + "] Disconnecting from Central Server");
            CacheCentralNetworkManager.StopClient();
        }

        public void OnCentralServerConnected(NetPeer netPeer)
        {
            Debug.Log("[" + peerType + "] Connected to Central Server");
            var peerInfo = new CentralServerPeerInfo();
            peerInfo.peerType = peerType;
            peerInfo.networkAddress = machineAddress;
            peerInfo.networkPort = networkPort;
            peerInfo.extra = extra;
            CacheCentralNetworkManager.RequestAppServerRegister(peerInfo, OnAppServerRegistered);
        }

        public void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("[" + peerType + "] Disconnected from Central Server");
            RegisteredToCentralServer = false;
            StartCoroutine(CentralServerReconnectRoutine());
        }

        IEnumerator CentralServerReconnectRoutine()
        {
            Debug.Log("[" + peerType + "] Reconnect to central in " + RECONNECT_DELAY + " seconds");
            yield return new WaitForSeconds(RECONNECT_DELAY);
            ConnectToCentralServer();
        }

        public void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message)
        {
            if (responseCode == AckResponseCode.Success)
                RegisteredToCentralServer = true;
            if (onAppServerRegistered != null)
                onAppServerRegistered(responseCode, message);
        }
    }
}
