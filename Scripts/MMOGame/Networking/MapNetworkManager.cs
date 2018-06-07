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

        private CentralAppServerConnector cacheCentralAppServerConnector;
        public CentralAppServerConnector CentralAppServerConnector
        {
            get
            {
                if (cacheCentralAppServerConnector == null)
                    cacheCentralAppServerConnector = gameObject.AddComponent<CentralAppServerConnector>();
                return cacheCentralAppServerConnector;
            }
        }

        private RpgGameManager cacheGameManager;
        public RpgGameManager GameManager
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
            GameManager.Init(this);
            return base.StartServer();
        }

        public override LiteNetLibClient StartClient()
        {
            GameManager.Init(this);
            return base.StartClient();
        }

        public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(peer, disconnectInfo);
            GameManager.OnClientDisconnected(peer, disconnectInfo);
        }

        public override void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            GameManager.OnServerOnlineSceneLoaded();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            var extra = !string.IsNullOrEmpty(Assets.onlineScene.SceneName) ? Assets.onlineScene.SceneName : SceneManager.GetActiveScene().name;
            CentralAppServerConnector.OnStartServer(CentralServerPeerType.MapSpawnServer, networkPort, connectKey, extra);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            CentralAppServerConnector.OnStopServer();
        }
    }
}
