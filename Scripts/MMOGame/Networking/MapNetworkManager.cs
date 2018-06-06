using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    [RequireComponent(typeof(RpgGameManager))]
    public class MapNetworkManager : LiteNetLibGameManager
    {
        public static MapNetworkManager Singleton { get; protected set; }
        public const float RECONNECT_DELAY = 5f;
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapServer; } }
        [Header("App Server Configs")]
        public string machineAddress = "127.0.0.1";
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

        private RpgGameManager cacheGameManager;
        public RpgGameManager CacheGameManager
        {
            get
            {
                if (cacheGameManager == null)
                    cacheGameManager = GetComponent<RpgGameManager>();
                return cacheGameManager;
            }
        }

        protected override void Awake()
        {
            Singleton = this;
            doNotDestroyOnSceneChanges = true;
            base.Awake();
        }

        public override bool StartServer()
        {
            CacheGameManager.Init(this);
            return base.StartServer();
        }

        public override LiteNetLibClient StartClient()
        {
            CacheGameManager.Init(this);
            return base.StartClient();
        }

        public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(peer, disconnectInfo);
            CacheGameManager.OnClientDisconnected(peer, disconnectInfo);
        }

        public override void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            CacheGameManager.OnServerOnlineSceneLoaded();
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
            CacheCentralNetworkManager.StartClient(centralServerAddress, centralServerPort);
        }

        public void DisconnectFromCentralServer()
        {
            Debug.Log("[" + PeerType + "] Starting server");
            CacheCentralNetworkManager.StopClient();
        }

        public virtual void OnCentralServerConnected(NetPeer netPeer)
        {
            Debug.Log("[" + PeerType + "] Connected from Central Server");
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
            return Assets.onlineScene;
        }
    }
}
