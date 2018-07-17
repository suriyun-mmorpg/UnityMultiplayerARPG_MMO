using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class MapNetworkManager : BaseGameNetworkManager, IAppServer
    {
        public static MapNetworkManager Singleton { get; protected set; }

        [Header("Central Network Connection")]
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";
        [Header("Database")]
        public float autoSaveDuration = 2f;

        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null)
                {
                    cacheCentralAppServerRegister = new CentralAppServerRegister(this);
                    cacheCentralAppServerRegister.onAppServerRegistered = OnAppServerRegistered;
                    cacheCentralAppServerRegister.RegisterMessage(MMOMessageTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
                }
                return cacheCentralAppServerRegister;
            }
        }

        private ChatNetworkManager cacheChatNetworkManager;
        public ChatNetworkManager ChatNetworkManager
        {
            get
            {
                if (cacheChatNetworkManager == null)
                    cacheChatNetworkManager = gameObject.AddComponent<ChatNetworkManager>();
                return cacheChatNetworkManager;
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
        private float lastSaveCharacterTime;
        private float lastSaveWorldTime;
        private Task saveCharactersTask;
        private Task saveWorldTask;
        // Listing
        private readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersBySceneName = new Dictionary<string, CentralServerPeerInfo>();
        private readonly Dictionary<long, SimpleUserCharacterData> users = new Dictionary<long, SimpleUserCharacterData>();

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
                if (Time.unscaledTime - lastSaveCharacterTime > autoSaveDuration)
                {
                    if (saveCharactersTask == null || saveCharactersTask.IsCompleted)
                    {
                        saveCharactersTask = SaveCharacters();
                        lastSaveCharacterTime = Time.unscaledTime;
                    }
                }
                if (Time.unscaledTime - lastSaveWorldTime > autoSaveDuration)
                {
                    if (saveWorldTask == null || saveWorldTask.IsCompleted)
                    {
                        saveWorldTask = SaveWorld();
                        lastSaveWorldTime = Time.unscaledTime;
                    }
                }
            }
        }

        protected override async void OnDestroy()
        {
            CentralAppServerRegister.Stop();
            // Wait old save character task to be completed
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await Task.WhenAll(saveCharactersTask, SaveCharacters());
            else
                await SaveCharacters();
            if (saveWorldTask != null && !saveWorldTask.IsCompleted)
                await Task.WhenAll(saveWorldTask, SaveWorld());
            else
                await SaveWorld();
            base.OnDestroy();
        }

        public override void UnregisterPlayerCharacter(NetPeer peer)
        {
            var connectId = peer.ConnectId;
            // Send remove character from map server
            SimpleUserCharacterData userData;
            if (users.TryGetValue(connectId, out userData))
            {
                users.Remove(connectId);
                // Remove map user from central server and chat server
                UpdateMapUser(CentralAppServerRegister.Peer, UpdateMapUserMessage.UpdateType.Remove, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client.Peer, UpdateMapUserMessage.UpdateType.Remove, userData);
            }
            base.UnregisterPlayerCharacter(peer);
        }

        public override async void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var connectId = peer.ConnectId;
            // Save player character data
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!playerCharacters.TryGetValue(connectId, out playerCharacterEntity))
            {
                var savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                saveCharactersTask = SaveCharacter(savingCharacterData);
                await saveCharactersTask;
            }
            UnregisterPlayerCharacter(peer);
            base.OnPeerDisconnected(peer, disconnectInfo);
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
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.StopClient();
            mapServerPeersBySceneName.Clear();
        }

        public override void OnClientConnected(NetPeer peer)
        {
            base.OnClientConnected(peer);
            if (onClientConnected != null)
                onClientConnected(peer);
        }

        public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(peer, disconnectInfo);
            if (onClientDisconnected != null)
                onClientDisconnected(peer, disconnectInfo);
        }

        public override async void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            base.WarpCharacter(playerCharacterEntity, mapName, position);
            long connectId = playerCharacterEntity.ConnectId;
            NetPeer peer;
            CentralServerPeerInfo peerInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                !mapName.Equals(playerCharacterEntity.CurrentMapName) &&
                playerCharacters.ContainsKey(connectId) &&
                Peers.TryGetValue(connectId, out peer) &&
                mapServerPeersBySceneName.TryGetValue(mapName, out peerInfo))
            {
                var message = new MMOWarpMessage();
                message.sceneName = mapName;
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                message.connectKey = peerInfo.connectKey;
                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.Warp, message);
                // Save character current map / position
                var savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                saveCharactersTask = SaveCharacter(savingCharacterData);
                await saveCharactersTask;
                // Unregister player character
                UnregisterPlayerCharacter(peer);
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
            }
        }

        #region Character spawn function
        public override void SerializeClientReadyExtra(NetDataWriter writer)
        {
            writer.Put(MMOClientInstance.UserId);
            writer.Put(MMOClientInstance.AccessToken);
            writer.Put(MMOClientInstance.SelectCharacterId);
        }

        public override async void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetPeer peer, NetDataReader reader)
        {
            var userId = reader.GetString();
            var accessToken = reader.GetString();
            var selectCharacterId = reader.GetString();
            // Validate access token
            if (playerCharacters.ContainsKey(peer.ConnectId))
            {
                Debug.LogError("[Map Server] User trying to hack: " + userId);
                Server.NetManager.DisconnectPeer(peer);
            }
            else if (!await Database.ValidateAccessToken(userId, accessToken))
            {
                Debug.LogError("[Map Server] Invalid access token for user: " + userId);
                Server.NetManager.DisconnectPeer(peer);
            }
            else
            {
                var playerCharacterData = await Database.ReadCharacter(userId, selectCharacterId);
                if (playerCharacterData == null)
                {
                    Debug.LogError("[Map Server] Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    Server.NetManager.DisconnectPeer(peer);
                    return;
                }
                var dataId = playerCharacterData.DataId;
                PlayerCharacter playerCharacter;
                if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out playerCharacter) || playerCharacter.entityPrefab == null)
                {
                    Debug.LogError("[Map Server] Cannot find player character with data Id: " + dataId);
                    return;
                }
                var playerCharacterPrefab = playerCharacter.entityPrefab;
                var identity = SpawnPlayer(peer, playerCharacterPrefab.Identity);
                var playerCharacterEntity = identity.GetComponent<BasePlayerCharacterEntity>();
                playerCharacterData.CloneTo(playerCharacterEntity);
                // Notify clients that this character is spawn or dead
                if (!playerCharacterEntity.IsDead())
                    playerCharacterEntity.RequestOnRespawn(true);
                else
                    playerCharacterEntity.RequestOnDead(true);
                RegisterPlayerCharacter(peer, playerCharacterEntity);
                var characterName = playerCharacterEntity.CharacterName;
                var userData = new SimpleUserCharacterData(userId, characterName);
                users[peer.ConnectId] = userData;
                // Add map user to central server and chat server
                UpdateMapUser(CentralAppServerRegister.Peer, UpdateMapUserMessage.UpdateType.Add, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client.Peer, UpdateMapUserMessage.UpdateType.Add, userData);
            }
        }
        #endregion

        #region Network message handlers
        protected override void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            // Send chat message to chat server, for MMO mode chat message handling by chat server
            var message = messageHandler.ReadMessage<ChatMessage>();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.EnterChat(message.channel, message.message, message.sender, message.receiver);
        }

        protected override void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<MMOWarpMessage>();
            Assets.offlineScene.SceneName = string.Empty;
            StopClient();
            Assets.onlineScene.SceneName = message.sceneName;
            StartClient(message.networkAddress, message.networkPort, message.connectKey);
        }

        private void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            if (message.responseCode == AckResponseCode.Success)
            {
                var peerInfo = message.peerInfo;
                switch (peerInfo.peerType)
                {
                    case CentralServerPeerType.MapServer:
                        if (!string.IsNullOrEmpty(peerInfo.extra))
                        {
                            Debug.Log("Register map server: " + peerInfo.extra);
                            mapServerPeersBySceneName[peerInfo.extra] = peerInfo;
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        if (!ChatNetworkManager.IsClientConnected)
                        {
                            Debug.Log("Connecting to chat server");
                            ChatNetworkManager.StartClient(this, peerInfo.networkAddress, peerInfo.networkPort, peerInfo.connectKey);
                        }
                        break;
                }
            }
        }

        private void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message)
        {
            if (responseCode == AckResponseCode.Success)
                UpdateMapUsers(CentralAppServerRegister.Peer, UpdateMapUserMessage.UpdateType.Add);
        }
        #endregion

        #region Connect to chat server
        public void OnChatServerConnected()
        {
            Debug.Log("Connected to chat server");
            UpdateMapUsers(ChatNetworkManager.Client.Peer, UpdateMapUserMessage.UpdateType.Add);
        }

        public void OnChatMessageReceive(ChatMessage message)
        {
            HandleChatAtServer(message);
        }
        #endregion

        #region Update map user functions
        private void UpdateMapUsers(NetPeer peer, UpdateMapUserMessage.UpdateType updateType)
        {
            foreach (var user in users.Values)
            {
                UpdateMapUser(peer, updateType, user);
            }
        }

        private void UpdateMapUser(NetPeer peer, UpdateMapUserMessage.UpdateType updateType, SimpleUserCharacterData userData)
        {
            var updateMapUserMessage = new UpdateMapUserMessage();
            updateMapUserMessage.type = updateType;
            updateMapUserMessage.userData = userData;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, peer, MMOMessageTypes.UpdateMapUser, updateMapUserMessage);
        }
        #endregion

        #region Save character functions
        private async Task SaveCharacter(IPlayerCharacterData playerCharacterData)
        {
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await saveCharactersTask;
            await Database.UpdateCharacter(playerCharacterData);
            Debug.Log("Character [" + playerCharacterData.Id + "] Saved");
        }

        private async Task SaveCharacters()
        {
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await saveCharactersTask;
            var tasks = new List<Task>();
            foreach (var playerCharacterEntity in playerCharacters.Values)
            {
                tasks.Add(Database.UpdateCharacter(playerCharacterEntity.CloneTo(new PlayerCharacterData())));
            }
            await Task.WhenAll(tasks);
            Debug.Log("Characters Saved");
        }

        private async Task SaveWorld()
        {
            // Save building entities / Tree / Rocks
            if (saveWorldTask != null && !saveWorldTask.IsCompleted)
                await saveWorldTask;
            var tasks = new List<Task>();
            foreach (var buildingEntity in buildingEntities.Values)
            {
                tasks.Add(Database.UpdateBuilding(Assets.onlineScene.SceneName, buildingEntity));
            }
            await Task.WhenAll(tasks);
        }

        public override async void CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            base.CreateBuildingEntity(saveData, initialize);
            if (!initialize)
                await Database.CreateBuilding(Assets.onlineScene.SceneName, saveData);
        }

        public override async void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            await Database.DeleteBuilding(Assets.onlineScene.SceneName, id);
        }

        public override async void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            // Spawn buildings
            var buildings = await Database.ReadBuildings(Assets.onlineScene.SceneName);
            foreach (var building in buildings)
            {
                CreateBuildingEntity(building, true);
            }
            // Spawn harvestables
            var harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (var harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.SpawnAll();
            }
        }
        #endregion
    }
}
