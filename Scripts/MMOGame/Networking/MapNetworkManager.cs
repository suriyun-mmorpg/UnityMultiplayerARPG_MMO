using System.Collections.Generic;
using System.Threading.Tasks;
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
        [Header("Database")]
        public float autoSaveDuration = 2f;

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null)
                {
                    cacheCentralAppServerRegister = new CentralAppServerRegister(this);
                    cacheCentralAppServerRegister.onAppServerRegistered = OnAppServerRegistered;
                }
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

        public BaseDatabase Database
        {
            get { return MMOServerInstance.Singleton.Database; }
        }

        public string CentralNetworkAddress { get { return centralNetworkAddress; } }
        public int CentralNetworkPort { get { return centralNetworkPort; } }
        public string CentralConnectKey { get { return centralConnectKey; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppConnectKey { get { return connectKey; } }
        public string AppExtra { get { return !string.IsNullOrEmpty(Assets.onlineScene.SceneName) ? Assets.onlineScene.SceneName : SceneManager.GetActiveScene().name; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapServer; } }
        private float lastSaveTime;
        private Task saveCharactersTask;
        private Dictionary<long, PlayerCharacterEntity> PlayerCharacterEntities = new Dictionary<long, PlayerCharacterEntity>();
        private Dictionary<long, string> UserIds = new Dictionary<long, string>();

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
            {
                CentralAppServerRegister.PollEvents();
                if (Time.unscaledTime - lastSaveTime > autoSaveDuration)
                {
                    if (saveCharactersTask == null || saveCharactersTask.IsCompleted)
                    {
                        saveCharactersTask = SaveCharacters();
                        lastSaveTime = Time.unscaledTime;
                    }
                }
            }
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            // Wait old save character task to be completed
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await Task.WhenAll(saveCharactersTask, SaveCharacters());
            else
                await SaveCharacters();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            PlayerCharacterEntity playerCharacterEntity;
            if (PlayerCharacterEntities.TryGetValue(peer.ConnectId, out playerCharacterEntity))
            {
                saveCharactersTask = SaveCharacter(playerCharacterEntity.CloneTo(new PlayerCharacterData()));
                PlayerCharacterEntities.Remove(peer.ConnectId);
            }
            string userId;
            if (UserIds.TryGetValue(peer.ConnectId, out userId))
            {
                var updateMapUserMessage = new RequestUpdateMapUserMessage();
                updateMapUserMessage.type = RequestUpdateMapUserMessage.UpdateType.Remove;
                updateMapUserMessage.userId = userId;
                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, CentralAppServerRegister.Peer, MessageTypes.RequestUpdateMapUser, updateMapUserMessage);
                UserIds.Remove(peer.ConnectId);
            }
            base.OnPeerDisconnected(peer, disconnectInfo);
        }

        private async Task SaveCharacter(IPlayerCharacterData playerCharacterData)
        {
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await saveCharactersTask;
            await Database.UpdateCharacter(playerCharacterData);
        }

        private async Task SaveCharacters()
        {
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await saveCharactersTask;
            var tasks = new List<Task>();
            foreach (var playerCharacterEntity in PlayerCharacterEntities.Values)
            {
                tasks.Add(Database.UpdateCharacter(playerCharacterEntity.CloneTo(new PlayerCharacterData())));
            }
            await Task.WhenAll(tasks);
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

        public override async void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetPeer peer, NetDataReader reader)
        {
            if (playerIdentity == null)
                return;

            var playerCharacterEntity = playerIdentity.GetComponent<PlayerCharacterEntity>();
            var userId = reader.GetString();
            var accessToken = reader.GetString();
            var selectCharacterId = reader.GetString();
            // Validate access token
            if (PlayerCharacterEntities.ContainsKey(peer.ConnectId))
            {
                Debug.LogError("[Map Server] User trying to hack: " + userId);
                Assets.NetworkDestroy(playerIdentity.ObjectId, DestroyObjectReasons.RequestedToDestroy);
                Server.NetManager.DisconnectPeer(peer);
            }
            else if (!await Database.ValidateAccessToken(userId, accessToken))
            {
                Debug.LogError("[Map Server] Invalid access token for user: " + userId);
                Assets.NetworkDestroy(playerIdentity.ObjectId, DestroyObjectReasons.RequestedToDestroy);
                Server.NetManager.DisconnectPeer(peer);
            }
            else
            {
                var playerCharacterData = await Database.ReadCharacter(userId, selectCharacterId);
                if (playerCharacterData == null)
                {
                    Debug.LogError("[Map Server] Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    Assets.NetworkDestroy(playerIdentity.ObjectId, DestroyObjectReasons.RequestedToDestroy);
                    Server.NetManager.DisconnectPeer(peer);
                    return;
                }
                playerCharacterData.CloneTo(playerCharacterEntity);
                // Notify clients that this character is spawn or dead
                if (playerCharacterEntity.CurrentHp > 0)
                    playerCharacterEntity.RequestOnRespawn(true);
                else
                    playerCharacterEntity.RequestOnDead(true);
                PlayerCharacterEntities[peer.ConnectId] = playerCharacterEntity;
                UserIds[peer.ConnectId] = userId;
                // Send update map user message to central server
                var updateMapUserMessage = new RequestUpdateMapUserMessage();
                updateMapUserMessage.type = RequestUpdateMapUserMessage.UpdateType.Add;
                updateMapUserMessage.userId = userId;
                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, CentralAppServerRegister.Peer, MessageTypes.RequestUpdateMapUser, updateMapUserMessage);
            }
        }

        private void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message)
        {
            if (responseCode == AckResponseCode.Success)
            {
                foreach (var userId in UserIds.Values)
                {
                    var updateMapUserMessage = new RequestUpdateMapUserMessage();
                    updateMapUserMessage.type = RequestUpdateMapUserMessage.UpdateType.Add;
                    updateMapUserMessage.userId = userId;
                    LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, CentralAppServerRegister.Peer, MessageTypes.RequestUpdateMapUser, updateMapUserMessage);
                }
            }
        }
    }
}
