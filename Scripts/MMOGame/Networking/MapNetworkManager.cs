using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager : BaseGameNetworkManager, IAppServer
    {
        public static MapNetworkManager Singleton { get; protected set; }

        [Header("Central Network Connection")]
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";
        [Header("Database")]
        public float autoSaveDuration = 2f;
        public float reloadPartyDuration = 5f;

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
        private readonly HashSet<int> loadingPartyIds = new HashSet<int>();
        private readonly Dictionary<int, float> lastLoadPartyTime = new Dictionary<int, float>();

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
                tempUnscaledTime = Time.unscaledTime;
                if (tempUnscaledTime - lastSaveCharacterTime > autoSaveDuration)
                {
                    if (saveCharactersTask == null || saveCharactersTask.IsCompleted)
                    {
                        saveCharactersTask = SaveCharacters();
                        lastSaveCharacterTime = tempUnscaledTime;
                    }
                }
                if (tempUnscaledTime - lastSaveWorldTime > autoSaveDuration)
                {
                    if (saveWorldTask == null || saveWorldTask.IsCompleted)
                    {
                        saveWorldTask = SaveWorld();
                        lastSaveWorldTime = tempUnscaledTime;
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
            if (playerCharacters.TryGetValue(connectId, out playerCharacterEntity))
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
                // If data is empty / cannot find character, disconnect user
                if (playerCharacterData == null)
                {
                    Debug.LogError("[Map Server] Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    Server.NetManager.DisconnectPeer(peer);
                    return;
                }
                // Load party data, if this map-server does not have party data
                if (playerCharacterData.PartyId > 0 && !parties.ContainsKey(playerCharacterData.PartyId))
                    await LoadPartyDataFromDatabase(playerCharacterData.PartyId);
                // If it is not allow this character data, disconnect user
                var dataId = playerCharacterData.DataId;
                PlayerCharacter playerCharacter;
                if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out playerCharacter) || playerCharacter.entityPrefab == null)
                {
                    Debug.LogError("[Map Server] Cannot find player character with data Id: " + dataId);
                    return;
                }
                // Spawn character entity and set its data
                var playerCharacterPrefab = playerCharacter.entityPrefab;
                var identity = Assets.NetworkSpawn(playerCharacterPrefab.Identity.HashAssetId, playerCharacterData.CurrentPosition, Quaternion.identity, 0, peer.ConnectId);
                var playerCharacterEntity = identity.GetComponent<BasePlayerCharacterEntity>();
                playerCharacterData.CloneTo(playerCharacterEntity);
                // Notify clients that this character is spawn or dead
                if (!playerCharacterEntity.IsDead())
                    playerCharacterEntity.RequestOnRespawn();
                else
                    playerCharacterEntity.RequestOnDead();
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
        protected override void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<MMOWarpMessage>();
            Assets.offlineScene.SceneName = string.Empty;
            StopClient();
            Assets.onlineScene.SceneName = message.sceneName;
            StartClient(message.networkAddress, message.networkPort, message.connectKey);
        }

        protected override void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            // Send chat message to chat server, for MMO mode chat message handling by chat server
            var message = messageHandler.ReadMessage<ChatMessage>();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.EnterChat(message.channel, message.message, message.sender, message.receiver);
        }

        protected override async void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashShopInfoMessage.Error.None;
            var cash = 0;
            var cashShopItemIds = new List<int>();
            SimpleUserCharacterData user;
            if (!users.TryGetValue(peer.ConnectId, out user))
                error = ResponseCashShopInfoMessage.Error.UserNotFound;
            else
            {
                cash = await Database.GetCash(user.userId);
                foreach (var cashShopItemId in GameInstance.CashShopItems.Keys)
                {
                    cashShopItemIds.Add(cashShopItemId);
                }
            }
            
            var responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            responseMessage.cashShopItemIds = cashShopItemIds.ToArray();
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashShopInfo, responseMessage);
        }

        protected override async void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            var error = ResponseCashShopBuyMessage.Error.None;
            var dataId = message.dataId;
            var cash = 0;
            SimpleUserCharacterData user;
            if (!users.TryGetValue(peer.ConnectId, out user))
                error = ResponseCashShopBuyMessage.Error.UserNotFound;
            else
            {
                // Request cash, reduce, send item info messages to map server
                cash = await Database.GetCash(user.userId);
                BasePlayerCharacterEntity playerCharacter;
                CashShopItem cashShopItem;
                if (!playerCharacters.TryGetValue(peer.ConnectId, out playerCharacter))
                    error = ResponseCashShopBuyMessage.Error.CharacterNotFound;
                else if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                    error = ResponseCashShopBuyMessage.Error.ItemNotFound;
                else if (cash < cashShopItem.sellPrice)
                    error = ResponseCashShopBuyMessage.Error.NotEnoughCash;
                else
                {
                    cash = await Database.DecreaseCash(user.userId, cashShopItem.sellPrice);
                    playerCharacter.Gold += cashShopItem.receiveGold;
                    foreach (var receiveItem in cashShopItem.receiveItems)
                    {
                        if (receiveItem.item == null) continue;
                        var characterItem = CharacterItem.Create(receiveItem.item, 1, receiveItem.amount);
                        playerCharacter.NonEquipItems.Add(characterItem);
                    }
                }
            }
            var responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashShopBuy, responseMessage);
        }

        protected override async void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashPackageInfoMessage.Error.None;
            var cash = 0;
            var cashPackageIds = new List<int>();
            SimpleUserCharacterData user;
            if (!users.TryGetValue(peer.ConnectId, out user))
                error = ResponseCashPackageInfoMessage.Error.UserNotFound;
            else
            {
                cash = await Database.GetCash(user.userId);
                foreach (var cashShopItemId in GameInstance.CashPackages.Keys)
                {
                    cashPackageIds.Add(cashShopItemId);
                }
            }

            var responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            responseMessage.cashPackageIds = cashPackageIds.ToArray();
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected override async void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            var error = ResponseCashPackageBuyValidationMessage.Error.None;
            var dataId = message.dataId;
            var cash = 0;
            SimpleUserCharacterData user;
            if (!users.TryGetValue(peer.ConnectId, out user))
                error = ResponseCashPackageBuyValidationMessage.Error.UserNotFound;
            else
            {
                // Get current cash will return this in case it cannot increase cash
                cash = await Database.GetCash(user.userId);
                // TODO: Validate purchasing at server side
                BasePlayerCharacterEntity playerCharacter;
                CashPackage cashPackage;
                if (!playerCharacters.TryGetValue(peer.ConnectId, out playerCharacter))
                    error = ResponseCashPackageBuyValidationMessage.Error.CharacterNotFound;
                else if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                    error = ResponseCashPackageBuyValidationMessage.Error.PackageNotFound;
                else
                    cash = await Database.IncreaseCash(user.userId, cashPackage.cashAmount);
            }
            var responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.dataId = dataId;
            responseMessage.cash = cash;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashPackageBuyValidation, responseMessage);
        }

        protected override async void HandleRequestPartyInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var responseMessage = new ResponsePartyInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            BasePlayerCharacterEntity playerCharacterEntity;
            PartyData partyData;
            if (playerCharacters.TryGetValue(peer.ConnectId, out playerCharacterEntity))
            {
                await LoadPartyDataFromDatabase(playerCharacterEntity.PartyId);
                // Set character party id to 0 if there is no party info with defined Id
                if (parties.TryGetValue(playerCharacterEntity.PartyId, out partyData))
                    responseMessage.members = partyData.GetMembers().ToArray();
                else
                    playerCharacterEntity.PartyId = 0;
            }
            LiteNetLibPacketSender.SendPacket(SendOptions.Sequenced, peer, MsgTypes.PartyInfo, responseMessage);
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

        #region Load Functions
        private async Task LoadPartyDataFromDatabase(int partyId)
        {
            // If there are other party loading which is not completed, it will not load again
            if (partyId <= 0 || loadingPartyIds.Contains(partyId))
                return;
            // If it is loading too frequently, skip it
            if (lastLoadPartyTime.ContainsKey(partyId) && Time.unscaledTime - lastLoadPartyTime[partyId] <= reloadPartyDuration)
                return;
            loadingPartyIds.Add(partyId);
            var party = await Database.ReadParty(partyId);
            if (party != null)
                parties[partyId] = party;
            lastLoadPartyTime[partyId] = Time.unscaledTime;
            loadingPartyIds.Remove(partyId);
        }
        #endregion

        #region Save functions
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
            foreach (var playerCharacterEntity in playerCharacters)
            {
                tasks.Add(Database.UpdateCharacter(playerCharacterEntity.Value));
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
            foreach (var buildingEntity in buildingEntities)
            {
                tasks.Add(Database.UpdateBuilding(Assets.onlineScene.SceneName, buildingEntity.Value));
            }
            await Task.WhenAll(tasks);
            Debug.Log("World Saved");
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

        #region Implement Abstract Functions
        public override async void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            // If warping to same map player does not have to reload new map data
            if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
            {
                playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
                return;
            }
            // If warping to different map
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

        public override async void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = await Database.CreateParty(shareExp, shareItem, playerCharacterEntity.Id);
            var party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
            await Database.SetCharacterParty(playerCharacterEntity.Id, partyId);
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
        }

        public override async void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            await Database.UpdateParty(playerCharacterEntity.PartyId, shareExp, shareItem);
            party.Setting(shareExp, shareItem);
            parties[partyId] = party;
        }

        public override async void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return;
            var partyId = inviteCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            if (party.CountMember() == gameInstance.maxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return;
            }
            await Database.SetCharacterParty(acceptCharacterEntity.Id, partyId);
            party.AddMember(acceptCharacterEntity);
            parties[partyId] = party;
            acceptCharacterEntity.PartyId = partyId;
        }

        public override async void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            await Database.SetCharacterParty(characterId, 0);
            party.RemoveMember(characterId);
            parties[partyId] = party;
        }

        public override async void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            // If it is leader kick all members and terminate party
            if (!party.IsLeader(playerCharacterEntity))
            {
                var tasks = new List<Task>();
                foreach (var memberId in party.GetMemberIds())
                {
                    tasks.Add(Database.SetCharacterParty(memberId, 0));
                }
                await Task.WhenAll(tasks);
                parties.Remove(partyId);
            }
            else
            {
                await Database.SetCharacterParty(playerCharacterEntity.Id, 0);
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
            }
        }
        #endregion
    }
}
