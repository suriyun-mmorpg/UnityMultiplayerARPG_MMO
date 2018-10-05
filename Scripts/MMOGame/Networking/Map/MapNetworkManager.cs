using System.Collections;
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

        public System.Action onClientConnected;
        public System.Action<DisconnectInfo> onClientDisconnected;

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
        private float lastSaveBuildingTime;
        // Listing
        private readonly Dictionary<string, CentralServerPeerInfo> mapServerConnectionIdsBySceneName = new Dictionary<string, CentralServerPeerInfo>();
        private readonly Dictionary<string, UserCharacterData> usersById = new Dictionary<string, UserCharacterData>();

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
                CentralAppServerRegister.Update();
                tempUnscaledTime = Time.unscaledTime;
                if (tempUnscaledTime - lastSaveCharacterTime > autoSaveDuration)
                {
                    StartCoroutine(SaveCharactersRoutine());
                    lastSaveCharacterTime = tempUnscaledTime;
                }
                if (tempUnscaledTime - lastSaveBuildingTime > autoSaveDuration)
                {
                    StartCoroutine(SaveBuildingsRoutine());
                    lastSaveBuildingTime = tempUnscaledTime;
                }
            }
        }

        protected override void UpdateOnlineCharacters(float time)
        {
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;

            tempCharacterIdArray = playerCharactersById.Keys.ToArray();
            foreach (var characterId in tempCharacterIdArray)
            {
                playerCharacter = playerCharactersById[characterId];
                if (usersById.TryGetValue(characterId, out userData))
                {
                    if (ChatNetworkManager.IsClientConnected)
                    {
                        userData.dataId = playerCharacter.DataId;
                        userData.level = playerCharacter.Level;
                        userData.currentHp = playerCharacter.CurrentHp;
                        userData.maxHp = playerCharacter.CacheMaxHp;
                        userData.currentMp = playerCharacter.CurrentMp;
                        userData.maxMp = playerCharacter.CacheMaxMp;
                        usersById[characterId] = userData;
                        UpdateMapUser(ChatNetworkManager.Client, UpdateMapUserMessage.UpdateType.Online, userData);
                    }
                }
            }

            tempPartyDataArray = parties.Values.ToArray();
            foreach (var party in tempPartyDataArray)
            {
                tempCharacterIdArray = party.GetMemberIds().ToArray();
                foreach (var memberId in tempCharacterIdArray)
                {
                    if (usersById.TryGetValue(memberId, out userData))
                    {
                        party.UpdateMember(userData.ConvertToSocialCharacterData());
                        party.NotifyMemberOnline(memberId, time);
                    }
                    party.UpdateMemberOnline(memberId, time);
                }
            }

            tempGuildDataArray = guilds.Values.ToArray();
            foreach (var guild in tempGuildDataArray)
            {
                tempCharacterIdArray = guild.GetMemberIds().ToArray();
                foreach (var memberId in tempCharacterIdArray)
                {
                    if (usersById.TryGetValue(memberId, out userData))
                    {
                        guild.UpdateMember(userData.ConvertToSocialCharacterData());
                        guild.NotifyMemberOnline(memberId, time);
                    }
                    guild.UpdateMemberOnline(memberId, time);
                }
            }
        }

        protected override void OnDestroy()
        {
            // Save immediately
            if (IsServer)
            {
                foreach (var playerCharacter in playerCharacters.Values)
                {
                    Database.UpdateCharacter(playerCharacter.CloneTo(new PlayerCharacterData()));
                }
                var sceneName = Assets.onlineScene.SceneName;
                foreach (var buildingEntity in buildingEntities.Values)
                {
                    Database.UpdateBuilding(sceneName, buildingEntity.CloneTo(new BuildingSaveData()));
                }
            }
            base.OnDestroy();
        }

        public override void UnregisterPlayerCharacter(long connectionId)
        {
            // Send remove character from map server
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (playerCharacters.TryGetValue(connectionId, out playerCharacter) && 
                usersById.TryGetValue(playerCharacter.Id, out userData))
            {
                usersById.Remove(playerCharacter.Id);
                // Remove map user from central server and chat server
                UpdateMapUser(CentralAppServerRegister, UpdateMapUserMessage.UpdateType.Remove, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client, UpdateMapUserMessage.UpdateType.Remove, userData);
            }
            base.UnregisterPlayerCharacter(connectionId);
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            StartCoroutine(OnPeerDisconnectedRoutine(connectionId, disconnectInfo));
        }

        private IEnumerator OnPeerDisconnectedRoutine(long connectionId, DisconnectInfo disconnectInfo)
        {
            // Save player character data
            BasePlayerCharacterEntity playerCharacterEntity;
            if (playerCharacters.TryGetValue(connectionId, out playerCharacterEntity))
            {
                var saveCharacterData = playerCharacterEntity.CloneTo(new PlayerCharacterData());
                while (savingCharacters.Contains(saveCharacterData.Id))
                {
                    yield return 0;
                }
                yield return StartCoroutine(SaveCharacterRoutine(saveCharacterData));
            }
            UnregisterPlayerCharacter(connectionId);
            base.OnPeerDisconnected(connectionId, disconnectInfo);
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
            mapServerConnectionIdsBySceneName.Clear();
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            if (onClientConnected != null)
                onClientConnected.Invoke();
        }

        public override void OnClientDisconnected(DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(disconnectInfo);
            if (onClientDisconnected != null)
                onClientDisconnected.Invoke(disconnectInfo);
        }

        public override void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            StartCoroutine(OnServerOnlineSceneLoadedRoutine());
        }

        private IEnumerator OnServerOnlineSceneLoadedRoutine()
        {
            // Spawn buildings
            var job = new ReadBuildingsJob(Database, Assets.onlineScene.SceneName);
            job.Start();
            yield return StartCoroutine(job.WaitFor());
            var buildings = job.result;
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

        #region Character spawn function
        public override void SerializeClientReadyExtra(NetDataWriter writer)
        {
            writer.Put(MMOClientInstance.UserId);
            writer.Put(MMOClientInstance.AccessToken);
            writer.Put(MMOClientInstance.SelectCharacterId);
        }

        public override void SetPlayerReady(long connectionId, NetDataReader reader)
        {
            if (!IsServer)
                return;

            var player = Players[connectionId];
            if (player.IsReady)
                return;

            player.IsReady = true;

            var userId = reader.GetString();
            var accessToken = reader.GetString();
            var selectCharacterId = reader.GetString();

            if (playerCharacters.ContainsKey(connectionId))
            {
                Debug.LogError("[Map Server] User trying to hack: " + userId);
                transport.ServerDisconnect(connectionId);
                return;
            }

            StartCoroutine(SetPlayerReadyRoutine(connectionId, userId, accessToken, selectCharacterId));
        }

        private IEnumerator SetPlayerReadyRoutine(long connectionId, string userId, string accessToken, string selectCharacterId)
        {
            // Validate access token
            var validateAccessTokenJob = new ValidateAccessTokenJob(Database, userId, accessToken);
            validateAccessTokenJob.Start();
            yield return StartCoroutine(validateAccessTokenJob.WaitFor());
            if (!validateAccessTokenJob.result)
            {
                Debug.LogError("[Map Server] Invalid access token for user: " + userId);
                transport.ServerDisconnect(connectionId);
            }
            else
            {
                var loadCharacterJob = new ReadCharacterJob(Database, userId, selectCharacterId);
                loadCharacterJob.Start();
                yield return StartCoroutine(loadCharacterJob.WaitFor());
                var playerCharacterData = loadCharacterJob.result;
                // If data is empty / cannot find character, disconnect user
                if (playerCharacterData == null)
                {
                    Debug.LogError("[Map Server] Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    transport.ServerDisconnect(connectionId);
                }
                else
                {
                    // If it is not allow this character data, disconnect user
                    var dataId = playerCharacterData.DataId;
                    PlayerCharacter playerCharacter;
                    if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out playerCharacter) || playerCharacter.entityPrefab == null)
                    {
                        Debug.LogError("[Map Server] Cannot find player character with data Id: " + dataId);
                    }
                    else
                    {
                        // Spawn character entity and set its data
                        var playerCharacterPrefab = playerCharacter.entityPrefab;
                        var identity = Assets.NetworkSpawn(playerCharacterPrefab.Identity.HashAssetId, playerCharacterData.CurrentPosition, Quaternion.identity, 0, connectionId);
                        var playerCharacterEntity = identity.GetComponent<BasePlayerCharacterEntity>();
                        playerCharacterData.CloneTo(playerCharacterEntity);
                        
                        // Load party data, if this map-server does not have party data
                        if (playerCharacterEntity.PartyId > 0 && !parties.ContainsKey(playerCharacterEntity.PartyId))
                        {
                            yield return StartCoroutine(LoadPartyRoutine(playerCharacterEntity.PartyId));
                            playerCharacterEntity.PartyMemberFlags = parties[playerCharacterEntity.PartyId].GetPartyMemberFlags(playerCharacterEntity);
                        }

                        // Load guild data, if this map-server does not have guild data
                        if (playerCharacterEntity.GuildId > 0 && !guilds.ContainsKey(playerCharacterEntity.GuildId))
                        {
                            yield return StartCoroutine(LoadGuildRoutine(playerCharacterEntity.GuildId));
                            byte guildRole;
                            playerCharacterEntity.GuildMemberFlags = guilds[playerCharacterEntity.GuildId].GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
                            playerCharacterEntity.GuildRole = guildRole;
                        }

                        // Notify clients that this character is spawn or dead
                        if (!playerCharacterEntity.IsDead())
                            playerCharacterEntity.RequestOnRespawn();
                        else
                            playerCharacterEntity.RequestOnDead();
                        RegisterPlayerCharacter(connectionId, playerCharacterEntity);
                        var userData = new UserCharacterData();
                        userData.userId = userId;
                        userData.id = playerCharacterEntity.Id;
                        userData.characterName = playerCharacterEntity.CharacterName;
                        userData.dataId = playerCharacterEntity.DataId;
                        userData.level = playerCharacterEntity.Level;
                        userData.currentHp = playerCharacterEntity.CurrentHp;
                        userData.maxHp = playerCharacterEntity.CacheMaxHp;
                        userData.currentMp = playerCharacterEntity.CurrentMp;
                        userData.maxMp = playerCharacterEntity.CacheMaxMp;
                        usersById[userData.id] = userData;
                        // Add map user to central server and chat server
                        UpdateMapUser(CentralAppServerRegister, UpdateMapUserMessage.UpdateType.Add, userData);
                        if (ChatNetworkManager.IsClientConnected)
                            UpdateMapUser(ChatNetworkManager.Client, UpdateMapUserMessage.UpdateType.Add, userData);

                        var player = Players[connectionId];
                        foreach (var spawnedObject in Assets.SpawnedObjects)
                        {
                            if (spawnedObject.Value.ConnectionId == player.ConnectionId)
                                continue;

                            if (spawnedObject.Value.ShouldAddSubscriber(player))
                                spawnedObject.Value.AddSubscriber(player);
                        }
                    }
                }
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
            if (ChatNetworkManager.IsClientConnected)
            {
                var message = FillChatChannelId(messageHandler.ReadMessage<ChatMessage>());
                ChatNetworkManager.EnterChat(message.channel, message.message, message.sender, message.receiver, message.channelId);
            }
        }

        protected override void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashShopInfoRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashShopInfoRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashShopInfoMessage.Error.None;
            var cash = 0;
            var cashShopItemIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashShopInfoMessage.Error.UserNotFound;
            else
            {
                var job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
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
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashShopInfo, responseMessage);
        }

        protected override void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashShopBuyRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashShopBuyRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            var error = ResponseCashShopBuyMessage.Error.None;
            var dataId = message.dataId;
            var cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashShopBuyMessage.Error.UserNotFound;
            else
            {
                // Request cash, reduce, send item info messages to map server
                var job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                    error = ResponseCashShopBuyMessage.Error.ItemNotFound;
                else if (cash < cashShopItem.sellPrice)
                    error = ResponseCashShopBuyMessage.Error.NotEnoughCash;
                else
                {
                    var decreaseCashJob = new DecreaseCashJob(Database, userData.userId, cashShopItem.sellPrice);
                    decreaseCashJob.Start();
                    yield return StartCoroutine(decreaseCashJob.WaitFor());
                    cash = decreaseCashJob.result;
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
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashShopBuy, responseMessage);
        }

        protected override void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashPackageInfoRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashPackageInfoRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashPackageInfoMessage.Error.None;
            var cash = 0;
            var cashPackageIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashPackageInfoMessage.Error.UserNotFound;
            else
            {
                var job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
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
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected override void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashPackageBuyValidationRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashPackageBuyValidationRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            var error = ResponseCashPackageBuyValidationMessage.Error.None;
            var dataId = message.dataId;
            var cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashPackageBuyValidationMessage.Error.UserNotFound;
            else
            {
                // Get current cash will return this in case it cannot increase cash
                var job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
                // TODO: Validate purchasing at server side
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                    error = ResponseCashPackageBuyValidationMessage.Error.PackageNotFound;
                else
                {
                    var increaseCashJob = new IncreaseCashJob(Database, userData.userId, cashPackage.cashAmount);
                    increaseCashJob.Start();
                    yield return StartCoroutine(increaseCashJob.WaitFor());
                    cash = increaseCashJob.result;
                }
            }
            var responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.dataId = dataId;
            responseMessage.cash = cash;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashPackageBuyValidation, responseMessage);
        }

        private void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
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
                            mapServerConnectionIdsBySceneName[peerInfo.extra] = peerInfo;
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
                UpdateMapUsers(CentralAppServerRegister, UpdateMapUserMessage.UpdateType.Add);
        }
        #endregion

        #region Connect to chat server
        public void OnChatServerConnected()
        {
            Debug.Log("Connected to chat server");
            UpdateMapUsers(ChatNetworkManager.Client, UpdateMapUserMessage.UpdateType.Add);
        }

        public void OnChatMessageReceive(ChatMessage message)
        {
            ReadChatMessage(message);
        }

        public void OnUpdateMapUser(UpdateMapUserMessage message)
        {
            UserCharacterData userData;
            switch (message.type)
            {
                case UpdateMapUserMessage.UpdateType.Add:
                    if (!usersById.ContainsKey(message.id))
                    {
                        userData = new UserCharacterData();
                        userData.id = message.id;
                        userData.userId = message.userId;
                        userData.characterName = message.characterName;
                        userData.dataId = message.dataId;
                        userData.level = message.level;
                        usersById[userData.id] = userData;
                    }
                    break;
                case UpdateMapUserMessage.UpdateType.Remove:
                    if (usersById.TryGetValue(message.id, out userData))
                        usersById.Remove(userData.id);
                    break;
                case UpdateMapUserMessage.UpdateType.Online:
                    if (usersById.TryGetValue(message.id, out userData))
                    {
                        userData.level = message.level;
                        userData.currentHp = message.currentHp;
                        userData.maxHp = message.maxHp;
                        userData.currentMp = message.currentMp;
                        userData.maxMp = message.maxMp;
                        usersById[userData.id] = userData;
                    }
                    break;
            }
        }

        public void OnUpdatePartyMember(UpdateSocialMemberMessage message)
        {
            PartyData party;
            BasePlayerCharacterEntity playerCharacterEntity;
            if (parties.TryGetValue(message.id, out party) && UpdateSocialGroupMember(party, message))
            {
                switch (message.type)
                {
                    case UpdateSocialMemberMessage.UpdateType.Add:
                        if (playerCharactersById.TryGetValue(message.characterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.PartyId = message.id;
                            playerCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(playerCharacterEntity);
                        }
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (playerCharactersById.TryGetValue(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.ClearParty();
                        break;
                }
            }
        }

        public void OnUpdateParty(UpdatePartyMessage message)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            PartyData party;
            if (message.type == UpdatePartyMessage.UpdateType.Create)
            {
                party = new PartyData(message.id, message.shareExp, message.shareItem, message.characterId);
                parties[message.id] = party;
            }
            else if (parties.TryGetValue(message.id, out party))
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        party.SetLeader(message.characterId);
                        parties[message.id] = party;
                        if (TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(playerCharacterEntity);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        party.Setting(message.shareExp, message.shareItem);
                        parties[message.id] = party;
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        foreach (var memberId in party.GetMemberIds())
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                                playerCharacterEntity.ClearParty();
                        }
                        parties.Remove(message.id);
                        break;
                }
            }
        }

        public void OnUpdateGuildMember(UpdateSocialMemberMessage message)
        {
            GuildData guild;
            BasePlayerCharacterEntity playerCharacterEntity;
            if (guilds.TryGetValue(message.id, out guild) && UpdateSocialGroupMember(guild, message))
            {
                switch (message.type)
                {
                    case UpdateSocialMemberMessage.UpdateType.Add:
                        if (playerCharactersById.TryGetValue(message.characterId, out playerCharacterEntity))
                        {
                            byte guildRole;
                            playerCharacterEntity.GuildId = message.id;
                            playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
                            playerCharacterEntity.GuildRole = guildRole;
                        }
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (playerCharactersById.TryGetValue(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.ClearGuild();
                        break;
                }
            }
        }

        public void OnUpdateGuild(UpdateGuildMessage message)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            byte guildRole;
            GuildData guild;
            if (message.type == UpdateGuildMessage.UpdateType.Create)
            {
                guild = new GuildData(message.id, message.guildName, message.characterId);
                guilds[message.id] = guild;
            }
            else if (guilds.TryGetValue(message.id, out guild))
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        guild.SetLeader(message.characterId);
                        guilds[message.id] = guild;
                        if (TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
                            playerCharacterEntity.GuildRole = guildRole;
                        }
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        guild.guildMessage = message.guildMessage;
                        guilds[message.id] = guild;
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        guild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        guilds[message.id] = guild;
                        foreach (var memberId in guild.GetMemberIds())
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                            {
                                playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
                                playerCharacterEntity.GuildRole = guildRole;
                            }
                        }
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        guild.SetMemberRole(message.characterId, message.guildRole);
                        guilds[message.id] = guild;
                        if (TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
                            playerCharacterEntity.GuildRole = guildRole;
                        }
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        foreach (var memberId in guild.GetMemberIds())
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                                playerCharacterEntity.ClearGuild();
                        }
                        guilds.Remove(message.id);
                        break;
                }
            }
        }
        #endregion

        #region Update map user functions
        private void UpdateMapUsers(TransportHandler transportHandler, UpdateMapUserMessage.UpdateType updateType)
        {
            foreach (var user in usersById.Values)
            {
                UpdateMapUser(transportHandler, updateType, user);
            }
        }

        private void UpdateMapUser(TransportHandler transportHandler, UpdateMapUserMessage.UpdateType updateType, UserCharacterData userData)
        {
            var updateMapUserMessage = new UpdateMapUserMessage();
            updateMapUserMessage.type = updateType;
            updateMapUserMessage.id = userData.id;
            updateMapUserMessage.userId = userData.userId;
            updateMapUserMessage.characterName = userData.characterName;
            updateMapUserMessage.dataId = userData.dataId;
            updateMapUserMessage.level = userData.level;
            updateMapUserMessage.currentHp = userData.currentHp;
            updateMapUserMessage.maxHp = userData.maxHp;
            updateMapUserMessage.currentMp = userData.currentMp;
            updateMapUserMessage.maxMp = userData.maxMp;
            transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, MMOMessageTypes.UpdateMapUser, updateMapUserMessage.Serialize);
        }
        #endregion
    }
}
