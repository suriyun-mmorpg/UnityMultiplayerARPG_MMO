using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace Insthync.MMOG
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
        private float lastSaveTime;
        private Task saveCharactersTask;
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
            CentralAppServerRegister.Stop();
            // Wait old save character task to be completed
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await Task.WhenAll(saveCharactersTask, SaveCharacters());
            else
                await SaveCharacters();
            base.OnDestroy();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            UnregisterPlayerCharacter(peer);
            SimpleUserCharacterData userData;
            if (users.TryGetValue(peer.ConnectId, out userData))
            {
                users.Remove(peer.ConnectId);
                // Remove map user from central server and chat server
                UpdateMapUser(CentralAppServerRegister.Peer, UpdateMapUserMessage.UpdateType.Remove, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client.Peer, UpdateMapUserMessage.UpdateType.Remove, userData);
            }
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

        public override async void WarpCharacter(PlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
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
                peersByCharacterName.Remove(playerCharacterEntity.CharacterName);
                playerCharacters.Remove(connectId);
                var message = new MMOWarpMessage();
                message.sceneName = mapName;
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                message.connectKey = peerInfo.connectKey;
                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.Warp, message);
                var savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                saveCharactersTask = SaveCharacter(savingCharacterData);
                await saveCharactersTask;
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
            if (playerIdentity == null)
                return;

            var playerCharacterEntity = playerIdentity.GetComponent<PlayerCharacterEntity>();
            var userId = reader.GetString();
            var accessToken = reader.GetString();
            var selectCharacterId = reader.GetString();
            // Validate access token
            if (playerCharacters.ContainsKey(peer.ConnectId))
            {
                Debug.LogError("[Map Server] User trying to hack: " + userId);
                playerIdentity.NetworkDestroy();
                Server.NetManager.DisconnectPeer(peer);
            }
            else if (!await Database.ValidateAccessToken(userId, accessToken))
            {
                Debug.LogError("[Map Server] Invalid access token for user: " + userId);
                playerIdentity.NetworkDestroy();
                Server.NetManager.DisconnectPeer(peer);
            }
            else
            {
                var playerCharacterData = await Database.ReadCharacter(userId, selectCharacterId);
                if (playerCharacterData == null)
                {
                    Debug.LogError("[Map Server] Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    playerIdentity.NetworkDestroy();
                    Server.NetManager.DisconnectPeer(peer);
                    return;
                }
                playerCharacterData.CloneTo(playerCharacterEntity);
                // Notify clients that this character is spawn or dead
                if (playerCharacterEntity.CurrentHp > 0)
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
                            mapServerPeersBySceneName[peerInfo.extra] = peerInfo;
                        break;
                    case CentralServerPeerType.Chat:
                        if (!ChatNetworkManager.IsClientConnected)
                            ChatNetworkManager.StartClient(this, peerInfo.networkAddress, peerInfo.networkPort, peerInfo.connectKey);
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
            UpdateMapUsers(ChatNetworkManager.Client.Peer, UpdateMapUserMessage.UpdateType.Add);
        }

        public void OnChatMessageReceive(ChatMessage message)
        {
            // Filtering chat messages
            switch (message.channel)
            {
                case ChatChannel.Global:
                    // Send message to all clients
                    SendPacketToAllPeers(SendOptions.ReliableOrdered, MMOMessageTypes.Chat, message);
                    break;
                case ChatChannel.Whisper:
                    // Send message to client which have the character
                    NetPeer receiverPeer;
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        peersByCharacterName.TryGetValue(message.receiver, out receiverPeer))
                        LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, receiverPeer, MsgTypes.Chat, message);
                    break;
                case ChatChannel.Party:
                    // TODO: Implement this later when party system ready
                    break;
                case ChatChannel.Guild:
                    // TODO: Implement this later when guild system ready
                    break;
            }
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
        #endregion
    }
}
