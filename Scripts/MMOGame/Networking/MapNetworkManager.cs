using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    [RequireComponent(typeof(RpgGameManager))]
    public class MapNetworkManager : LiteNetLibGameManager, IAppServer
    {
        public static MapNetworkManager Singleton { get; protected set; }

        [Header("Central Network Connection")]
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null)
                    cacheCentralAppServerRegister = new CentralAppServerRegister(this);
                return cacheCentralAppServerRegister;
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

        public string CentralNetworkAddress { get { return centralNetworkAddress; } }
        public int CentralNetworkPort { get { return centralNetworkPort; } }
        public string CentralConnectKey { get { return centralConnectKey; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppConnectKey { get { return connectKey; } }
        public string AppExtra { get { return !string.IsNullOrEmpty(Assets.onlineScene.SceneName) ? Assets.onlineScene.SceneName : SceneManager.GetActiveScene().name; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapServer; } }

        protected override void Awake()
        {
            Singleton = this;
            doNotDestroyOnSceneChanges = true;
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
            if (IsServer)
                CentralAppServerRegister.PollEvents();
        }

        public override bool StartServer()
        {
            GameManager.Init(this);
            return base.StartServer();
        }

        public override LiteNetLibClient StartClient(string networkAddress, int networkPort, string connectKey)
        {
            GameManager.Init(this);
            return base.StartClient(networkAddress, networkPort, connectKey);
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
            CentralAppServerRegister.OnStartServer();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            CentralAppServerRegister.OnStopServer();
        }

        public override void SerializeClientReadyExtra(NetDataWriter writer)
        {
            writer.Put(MMOClientInstance.UserId);
            writer.Put(MMOClientInstance.AccessToken);
            writer.Put(MMOClientInstance.SelectCharacterId);
        }

        public override async void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetDataReader reader)
        {
            if (playerIdentity == null)
                return;

            var playerCharacterEntity = playerIdentity.GetComponent<PlayerCharacterEntity>();
            var userId = reader.GetString();
            var accessToken = reader.GetString();
            var selectCharacterId = reader.GetString();
            // TODO: Validate access token
            var playerCharacterData = await MMOServerInstance.Singleton.Database.ReadCharacter(userId, selectCharacterId);
            if (playerCharacterData == null)
            {
                Debug.LogError("[Map Server] Cannot find select character " + selectCharacterId);
                Assets.NetworkDestroy(playerIdentity.ObjectId, DestroyObjectReasons.RequestedToDestroy);
                return;
            }
            playerCharacterData.CloneTo(playerCharacterEntity);
            // Notify clients that this character is spawn or dead
            if (playerCharacterEntity.CurrentHp > 0)
                playerCharacterEntity.RequestOnRespawn(true);
            else
                playerCharacterEntity.RequestOnDead(true);
        }
    }
}
