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
        private CentralServerPeerType registerPeerType;
        private int registerNetworkPort;
        private string registerConnectKey;
        private string registerExtra;

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

        public void OnStartServer(CentralServerPeerType peerType, int networkPort, string connectKey, string extra)
        {
            registerPeerType = peerType;
            registerNetworkPort = networkPort;
            registerConnectKey = connectKey;
            registerExtra = extra;
            Debug.Log("[" + peerType + "] Starting server");
            ConnectToCentralServer();
        }

        public void OnStopServer()
        {
            Debug.Log("[" + registerPeerType + "] Stopping server");
            DisconnectFromCentralServer();
        }

        public void ConnectToCentralServer()
        {
            Debug.Log("[" + registerPeerType + "] Connecting to Central Server");
            CacheCentralNetworkManager.StartClient(centralServerAddress, centralServerPort, centralServerConnectKey);
        }

        public void DisconnectFromCentralServer()
        {
            Debug.Log("[" + registerPeerType + "] Disconnecting from Central Server");
            CacheCentralNetworkManager.StopClient();
        }

        public void OnCentralServerConnected(NetPeer netPeer)
        {
            Debug.Log("[" + registerPeerType + "] Connected to Central Server");
            var peerInfo = new CentralServerPeerInfo();
            peerInfo.peerType = registerPeerType;
            peerInfo.networkAddress = machineAddress;
            peerInfo.networkPort = registerNetworkPort;
            peerInfo.connectKey = registerConnectKey;
            peerInfo.extra = registerExtra;
            CacheCentralNetworkManager.RequestAppServerRegister(peerInfo, OnAppServerRegistered);
        }

        public void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("[" + registerPeerType + "] Disconnected from Central Server");
            RegisteredToCentralServer = false;
            StartCoroutine(CentralServerReconnectRoutine());
        }

        IEnumerator CentralServerReconnectRoutine()
        {
            Debug.Log("[" + registerPeerType + "] Reconnect to central in " + RECONNECT_DELAY + " seconds");
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
