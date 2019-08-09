using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public sealed partial class MapNetworkManager : BaseGameNetworkManager, IAppServer
    {
        public const float TERMINATE_INSTANCE_DELAY = 30f;  // Close instance when no clients connected within 30 seconds

        public string mapInstanceId;

        [Header("Central Network Connection")]
        public BaseTransportFactory centralTransportFactory;
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        [Header("Database")]
        public float autoSaveDuration = 2f;

        public System.Action onClientConnected;
        public System.Action<DisconnectInfo> onClientDisconnected;

        private float terminatingTime;

        public BaseTransportFactory CentralTransportFactory
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // Force to use websocket transport if it's running as webgl
                if (centralTransportFactory == null || !centralTransportFactory.CanUseWithWebGL)
                    centralTransportFactory = gameObject.AddComponent<WebSocketTransportFactory>();
#else
                if (centralTransportFactory == null)
                    centralTransportFactory = gameObject.AddComponent<LiteNetLibTransportFactory>();
#endif
                return centralTransportFactory;
            }
        }

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null && CentralTransportFactory != null)
                {
                    cacheCentralAppServerRegister = new CentralAppServerRegister(CentralTransportFactory.Build(), this);
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
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppExtra
        {
            get
            {
                if (IsInstanceMap())
                    return mapInstanceId;
                return CurrentMapInfo.Id;
            }
        }
        public CentralServerPeerType PeerType
        {
            get
            {
                if (IsInstanceMap())
                    return CentralServerPeerType.InstanceMapServer;
                return CentralServerPeerType.MapServer;
            }
        }
        private float lastSaveTime;
        // Listing
        private readonly Dictionary<uint, KeyValuePair<string, Vector3>> instanceMapCurrentLocations = new Dictionary<uint, KeyValuePair<string, Vector3>>();
        private readonly Dictionary<string, CentralServerPeerInfo> mapServerConnectionIdsBySceneName = new Dictionary<string, CentralServerPeerInfo>();
        private readonly Dictionary<string, CentralServerPeerInfo> instanceMapServerConnectionIdsByInstanceId = new Dictionary<string, CentralServerPeerInfo>();
        private readonly Dictionary<string, HashSet<uint>> instanceMapWarpingCharactersByInstanceId = new Dictionary<string, HashSet<uint>>();
        private readonly Dictionary<string, KeyValuePair<string, Vector3>> instanceMapWarpingLocations = new Dictionary<string, KeyValuePair<string, Vector3>>();
        private readonly Dictionary<string, UserCharacterData> usersById = new Dictionary<string, UserCharacterData>();
        private readonly Dictionary<StorageId, HashSet<uint>> usingStorageCharacters = new Dictionary<StorageId, HashSet<uint>>();
        // Database operations
        private readonly HashSet<int> loadingPartyIds = new HashSet<int>();
        private readonly HashSet<int> loadingGuildIds = new HashSet<int>();
        private readonly HashSet<string> savingCharacters = new HashSet<string>();
        private readonly HashSet<string> savingBuildings = new HashSet<string>();

        protected override void Update()
        {
            base.Update();
            float tempUnscaledTime = Time.unscaledTime;
            if (IsServer)
            {
                CentralAppServerRegister.Update();
                if (tempUnscaledTime - lastSaveTime > autoSaveDuration)
                {
                    lastSaveTime = tempUnscaledTime;
                    StartCoroutine(SaveCharactersRoutine());
                    if (!IsInstanceMap())
                    {
                        // Don't save building if it's instance map
                        StartCoroutine(SaveBuildingsRoutine());
                    }
                }
                if (IsInstanceMap())
                {
                    // Quitting application when no players
                    if (Players.Count > 0)
                        terminatingTime = tempUnscaledTime;
                    else if (tempUnscaledTime - terminatingTime >= TERMINATE_INSTANCE_DELAY)
                        Application.Quit();
                }
            }
        }

        protected override void Clean()
        {
            base.Clean();
            instanceMapCurrentLocations.Clear();
            mapServerConnectionIdsBySceneName.Clear();
            instanceMapServerConnectionIdsByInstanceId.Clear();
            instanceMapWarpingCharactersByInstanceId.Clear();
            instanceMapWarpingLocations.Clear();
            usersById.Clear();
            usingStorageCharacters.Clear();
            loadingPartyIds.Clear();
            loadingGuildIds.Clear();
            savingCharacters.Clear();
            savingBuildings.Clear();
        }

        protected override void UpdateOnlineCharacter(long connectionId, BasePlayerCharacterEntity playerCharacterEntity, float time)
        {
            base.UpdateOnlineCharacter(connectionId, playerCharacterEntity, time);

            UserCharacterData tempUserData;
            if (ChatNetworkManager.IsClientConnected && usersById.TryGetValue(playerCharacterEntity.Id, out tempUserData))
            {
                tempUserData.dataId = playerCharacterEntity.DataId;
                tempUserData.level = playerCharacterEntity.Level;
                tempUserData.currentHp = playerCharacterEntity.CurrentHp;
                tempUserData.maxHp = playerCharacterEntity.CacheMaxHp;
                tempUserData.currentMp = playerCharacterEntity.CurrentMp;
                tempUserData.maxMp = playerCharacterEntity.CacheMaxMp;
                tempUserData.partyId = playerCharacterEntity.PartyId;
                tempUserData.guildId = playerCharacterEntity.GuildId;
                usersById[playerCharacterEntity.Id] = tempUserData;
                UpdateMapUser(ChatNetworkManager.Client, UpdateUserCharacterMessage.UpdateType.Online, tempUserData);
            }
        }

        protected override void OnDestroy()
        {
            // Save immediately
            if (IsServer)
            {
                foreach (BasePlayerCharacterEntity playerCharacter in playerCharacters.Values)
                {
                    Database.UpdateCharacter(playerCharacter.CloneTo(new PlayerCharacterData()));
                }
                string sceneName = Assets.onlineScene.SceneName;
                foreach (BuildingEntity buildingEntity in buildingEntities.Values)
                {
                    if (buildingEntity == null) continue;
                    Database.UpdateBuilding(sceneName, buildingEntity.CloneTo(new BuildingSaveData()));
                }
            }
            base.OnDestroy();
        }

        public override void RegisterPlayerCharacter(long connectionId, BasePlayerCharacterEntity playerCharacterEntity)
        {
            // Set user data to map server
            if (!usersById.ContainsKey(playerCharacterEntity.Id))
            {
                UserCharacterData userData = new UserCharacterData();
                userData.userId = playerCharacterEntity.UserId;
                userData.id = playerCharacterEntity.Id;
                userData.characterName = playerCharacterEntity.CharacterName;
                userData.dataId = playerCharacterEntity.DataId;
                userData.level = playerCharacterEntity.Level;
                userData.currentHp = playerCharacterEntity.CurrentHp;
                userData.maxHp = playerCharacterEntity.CacheMaxHp;
                userData.currentMp = playerCharacterEntity.CurrentMp;
                userData.maxMp = playerCharacterEntity.CacheMaxMp;
                usersById.Add(userData.id, userData);
                // Add map user to central server and chat server
                UpdateMapUser(CentralAppServerRegister, UpdateUserCharacterMessage.UpdateType.Add, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client, UpdateUserCharacterMessage.UpdateType.Add, userData);
            }
            base.RegisterPlayerCharacter(connectionId, playerCharacterEntity);
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
                UpdateMapUser(CentralAppServerRegister, UpdateUserCharacterMessage.UpdateType.Remove, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client, UpdateUserCharacterMessage.UpdateType.Remove, userData);
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
                PlayerCharacterData saveCharacterData = playerCharacterEntity.CloneTo(new PlayerCharacterData());
                while (savingCharacters.Contains(saveCharacterData.Id))
                {
                    yield return 0;
                }
                yield return StartCoroutine(SaveCharacterRoutine(saveCharacterData));
            }
            UnregisterPlayerCharacter(connectionId);
            base.OnPeerDisconnected(connectionId, disconnectInfo);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            CentralAppServerRegister.OnStopServer();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.StopClient();
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
            yield return new WaitForSecondsRealtime(1);
            // Spawn buildings
            if (!IsInstanceMap())
            {
                // Load buildings
                // Don't load buildings if it's instance map
                ReadBuildingsJob job = new ReadBuildingsJob(Database, Assets.onlineScene.SceneName);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                List<BuildingSaveData> buildings = job.result;
                foreach (BuildingSaveData building in buildings)
                {
                    CreateBuildingEntity(building, true);
                }
            }
            // Spawn harvestables
            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.SpawnAll();
            }
            CentralAppServerRegister.OnStartServer();
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

            LiteNetLibPlayer player = GetPlayer(connectionId);
            if (player.IsReady)
                return;

            player.IsReady = true;

            string userId = reader.GetString();
            string accessToken = reader.GetString();
            string selectCharacterId = reader.GetString();

            if (playerCharacters.ContainsKey(connectionId))
            {
                if (LogError)
                    Debug.LogError("[Map Server] User trying to hack: " + userId);
                Transport.ServerDisconnect(connectionId);
                return;
            }

            StartCoroutine(SetPlayerReadyRoutine(connectionId, userId, accessToken, selectCharacterId));
        }

        private IEnumerator SetPlayerReadyRoutine(long connectionId, string userId, string accessToken, string selectCharacterId)
        {
            // Validate access token
            ValidateAccessTokenJob validateAccessTokenJob = new ValidateAccessTokenJob(Database, userId, accessToken);
            validateAccessTokenJob.Start();
            yield return StartCoroutine(validateAccessTokenJob.WaitFor());
            if (!validateAccessTokenJob.result)
            {
                if (LogError)
                    Debug.LogError("[Map Server] Invalid access token for user: " + userId);
                Transport.ServerDisconnect(connectionId);
            }
            else
            {
                ReadCharacterJob loadCharacterJob = new ReadCharacterJob(Database, userId, selectCharacterId);
                loadCharacterJob.Start();
                yield return StartCoroutine(loadCharacterJob.WaitFor());
                PlayerCharacterData playerCharacterData = loadCharacterJob.result;
                // If data is empty / cannot find character, disconnect user
                if (playerCharacterData == null)
                {
                    if (LogError)
                        Debug.LogError("[Map Server] Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    Transport.ServerDisconnect(connectionId);
                }
                else
                {
                    BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
                    // If it is not allow this character data, disconnect user
                    if (entityPrefab == null)
                    {
                        if (LogError)
                            Debug.LogError("[Map Server] Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                        Transport.ServerDisconnect(connectionId);
                    }
                    else
                    {
                        // Prepare saving location for this character
                        string savingCurrentMapName = playerCharacterData.CurrentMapName;
                        Vector3 savingCurrentPosition = playerCharacterData.CurrentPosition;

                        // Move character to map info → start position
                        if (IsInstanceMap())
                            playerCharacterData.CurrentPosition = CurrentMapInfo.startPosition;

                        // Spawn character entity and set its data
                        GameObject spawnObj = Instantiate(entityPrefab.gameObject, playerCharacterData.CurrentPosition, Quaternion.identity);
                        BasePlayerCharacterEntity playerCharacterEntity = spawnObj.GetComponent<BasePlayerCharacterEntity>();
                        playerCharacterData.CloneTo(playerCharacterEntity);
                        Assets.NetworkSpawn(spawnObj, 0, connectionId);

                        // Set currencies
                        GetGoldJob getGoldJob = new GetGoldJob(Database, userId, (amount) =>
                        {
                            playerCharacterEntity.UserGold = amount;
                        });
                        getGoldJob.Start();
                        StartCoroutine(getGoldJob.WaitFor());
                        GetCashJob getCashJob = new GetCashJob(Database, userId, (amount) =>
                        {
                            playerCharacterEntity.UserCash = amount;
                        });
                        getCashJob.Start();
                        StartCoroutine(getCashJob.WaitFor());

                        // Prepare saving location for this character
                        if (IsInstanceMap())
                            instanceMapCurrentLocations.Add(playerCharacterEntity.ObjectId, new KeyValuePair<string, Vector3>(savingCurrentMapName, savingCurrentPosition));

                        // Set user Id
                        playerCharacterEntity.UserId = userId;

                        // Load user level
                        GetUserLevelJob loadUserLevelJob = new GetUserLevelJob(Database, userId, (level) =>
                        {
                            playerCharacterEntity.UserLevel = level;
                        });
                        loadUserLevelJob.Start();
                        StartCoroutine(loadUserLevelJob.WaitFor());

                        // Load party data, if this map-server does not have party data
                        if (playerCharacterEntity.PartyId > 0)
                        {
                            if (!parties.ContainsKey(playerCharacterEntity.PartyId))
                                yield return StartCoroutine(LoadPartyRoutine(playerCharacterEntity.PartyId));
                            if (parties.ContainsKey(playerCharacterEntity.PartyId))
                            {
                                PartyData party = parties[playerCharacterEntity.PartyId];
                                SendCreatePartyToClient(playerCharacterEntity.ConnectionId, party);
                                SendAddPartyMembersToClient(playerCharacterEntity.ConnectionId, party);
                            }
                            else
                                playerCharacterEntity.ClearParty();
                        }

                        // Load guild data, if this map-server does not have guild data
                        if (playerCharacterEntity.GuildId > 0)
                        {
                            if (!guilds.ContainsKey(playerCharacterEntity.GuildId))
                                yield return StartCoroutine(LoadGuildRoutine(playerCharacterEntity.GuildId));
                            if (guilds.ContainsKey(playerCharacterEntity.GuildId))
                            {
                                GuildData guild = guilds[playerCharacterEntity.GuildId];
                                playerCharacterEntity.GuildName = guild.guildName;
                                playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                                SendCreateGuildToClient(playerCharacterEntity.ConnectionId, guild);
                                SendAddGuildMembersToClient(playerCharacterEntity.ConnectionId, guild);
                                SendSetGuildMessageToClient(playerCharacterEntity.ConnectionId, guild);
                                SendSetGuildRolesToClient(playerCharacterEntity.ConnectionId, guild);
                                SendSetGuildMemberRolesToClient(playerCharacterEntity.ConnectionId, guild);
                                SendSetGuildSkillLevelsToClient(playerCharacterEntity.ConnectionId, guild);
                                SendSetGuildGoldToClient(playerCharacterEntity.ConnectionId, guild);
                                SendGuildLevelExpSkillPointToClient(playerCharacterEntity.ConnectionId, guild);
                            }
                            else
                                playerCharacterEntity.ClearGuild();
                        }

                        // Summon saved summons
                        for (int i = 0; i < playerCharacterEntity.Summons.Count; ++i)
                        {
                            CharacterSummon summon = playerCharacterEntity.Summons[i];
                            summon.Summon(playerCharacterEntity, summon.Level, summon.summonRemainsDuration, summon.Exp, summon.CurrentHp, summon.CurrentMp);
                            playerCharacterEntity.Summons[i] = summon;
                        }
                        
                        // Notify clients that this character is spawn or dead
                        if (!playerCharacterEntity.IsDead())
                            playerCharacterEntity.RequestOnRespawn();
                        else
                            playerCharacterEntity.RequestOnDead();

                        // Register player character entity to the server
                        RegisterPlayerCharacter(connectionId, playerCharacterEntity);

                        // Setup subscribers
                        LiteNetLibPlayer player = GetPlayer(connectionId);
                        foreach (LiteNetLibIdentity spawnedObject in Assets.GetSpawnedObjects())
                        {
                            if (spawnedObject.ConnectionId == player.ConnectionId)
                                continue;

                            if (spawnedObject.ShouldAddSubscriber(player))
                                spawnedObject.AddSubscriber(player);
                        }
                    }
                }
            }
        }
        #endregion

        #region Network message handlers
        protected override void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
        {
            MMOWarpMessage message = messageHandler.ReadMessage<MMOWarpMessage>();
            Assets.offlineScene.SceneName = string.Empty;
            StopClient();
            StartClient(message.networkAddress, message.networkPort);
        }

        protected override void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            ChatMessage message = FillChatChannelId(messageHandler.ReadMessage<ChatMessage>());
            // Local chat will processes immediately, not have to be sent to chat server
            if (message.channel == ChatChannel.Local)
            {
                ReadChatMessage(message);
                return;
            }
            // Send chat message to chat server, for MMO mode chat message handling by chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.Client.SendEnterChat(null, MMOMessageTypes.Chat, message.channel, message.message, message.sender, message.receiver, message.channelId);
            }
        }

        protected override void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashShopInfoRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashShopInfoRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            // Set response data
            ResponseCashShopInfoMessage.Error error = ResponseCashShopInfoMessage.Error.None;
            int cash = 0;
            List<int> cashShopItemIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashShopInfoMessage.Error.UserNotFound;
            else
            {
                GetCashJob job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
                foreach (int cashShopItemId in GameInstance.CashShopItems.Keys)
                {
                    cashShopItemIds.Add(cashShopItemId);
                }
            }
            // Send response message
            ResponseCashShopInfoMessage responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            responseMessage.cashShopItemIds = cashShopItemIds.ToArray();
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashShopInfo, responseMessage);
        }

        protected override void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashShopBuyRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashShopBuyRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashShopBuyMessage message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            // Set response data
            ResponseCashShopBuyMessage.Error error = ResponseCashShopBuyMessage.Error.None;
            int dataId = message.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashShopBuyMessage.Error.UserNotFound;
            else
            {
                // Request cash, reduce, send item info messages to map server
                GetCashJob job = new GetCashJob(Database, userData.userId);
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
                    DecreaseCashJob decreaseCashJob = new DecreaseCashJob(Database, userData.userId, cashShopItem.sellPrice);
                    decreaseCashJob.Start();
                    yield return StartCoroutine(decreaseCashJob.WaitFor());
                    cash = decreaseCashJob.result;
                    playerCharacter.Gold += cashShopItem.receiveGold;
                    foreach (ItemAmount receiveItem in cashShopItem.receiveItems)
                    {
                        if (receiveItem.item == null || receiveItem.amount <= 0) continue;
                        playerCharacter.AddOrInsertNonEquipItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
                    }
                }
            }
            // Send response message
            ResponseCashShopBuyMessage responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.dataId = dataId;
            responseMessage.cash = cash;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashShopBuy, responseMessage);
        }

        protected override void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashPackageInfoRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashPackageInfoRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            // Set response data
            ResponseCashPackageInfoMessage.Error error = ResponseCashPackageInfoMessage.Error.None;
            int cash = 0;
            List<int> cashPackageIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashPackageInfoMessage.Error.UserNotFound;
            else
            {
                GetCashJob job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
                foreach (int cashShopItemId in GameInstance.CashPackages.Keys)
                {
                    cashPackageIds.Add(cashShopItemId);
                }
            }
            // Send response message
            ResponseCashPackageInfoMessage responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = cash;
            responseMessage.cashPackageIds = cashPackageIds.ToArray();
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected override void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCashPackageBuyValidationRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCashPackageBuyValidationRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashPackageBuyValidationMessage message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            // TODO: Validate purchasing at server side
            // Set response data
            ResponseCashPackageBuyValidationMessage.Error error = ResponseCashPackageBuyValidationMessage.Error.None;
            int dataId = message.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            UserCharacterData userData;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter) ||
                !usersById.TryGetValue(playerCharacter.Id, out userData))
                error = ResponseCashPackageBuyValidationMessage.Error.UserNotFound;
            else
            {
                // Get current cash will return this in case it cannot increase cash
                GetCashJob job = new GetCashJob(Database, userData.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                cash = job.result;
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                    error = ResponseCashPackageBuyValidationMessage.Error.PackageNotFound;
                else
                {
                    IncreaseCashJob increaseCashJob = new IncreaseCashJob(Database, userData.userId, cashPackage.cashAmount);
                    increaseCashJob.Start();
                    yield return StartCoroutine(increaseCashJob.WaitFor());
                    cash = increaseCashJob.result;
                }
            }
            // Send response message
            ResponseCashPackageBuyValidationMessage responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.dataId = dataId;
            responseMessage.cash = cash;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageBuyValidation, responseMessage);
        }

        private void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            ResponseAppServerAddressMessage message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            if (message.responseCode == AckResponseCode.Success)
            {
                CentralServerPeerInfo peerInfo = message.peerInfo;
                switch (peerInfo.peerType)
                {
                    case CentralServerPeerType.MapServer:
                        if (!string.IsNullOrEmpty(peerInfo.extra))
                        {
                            if (LogInfo)
                                Debug.Log("Register map server: " + peerInfo.extra);
                            mapServerConnectionIdsBySceneName[peerInfo.extra] = peerInfo;
                        }
                        break;
                    case CentralServerPeerType.InstanceMapServer:
                        if (!string.IsNullOrEmpty(peerInfo.extra))
                        {
                            if (LogInfo)
                                Debug.Log("Register instance map server: " + peerInfo.extra);
                            instanceMapServerConnectionIdsByInstanceId[peerInfo.extra] = peerInfo;
                            // Warp characters
                            HashSet<uint> warpingCharacters = new HashSet<uint>();
                            if (instanceMapWarpingCharactersByInstanceId.TryGetValue(peerInfo.extra, out warpingCharacters))
                            {
                                BasePlayerCharacterEntity warpingCharacterEntity;
                                foreach (uint warpingCharacter in warpingCharacters)
                                {
                                    if (!Assets.TryGetSpawnedObject(warpingCharacter, out warpingCharacterEntity))
                                        continue;
                                    StartCoroutine(WarpCharacterToInstanceRoutine(warpingCharacterEntity, peerInfo.extra));
                                }
                            }
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        if (!ChatNetworkManager.IsClientConnected)
                        {
                            if (LogInfo)
                                Debug.Log("Connecting to chat server");
                            ChatNetworkManager.StartClient(this, peerInfo.networkAddress, peerInfo.networkPort);
                        }
                        break;
                }
            }
        }

        private void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message)
        {
            if (responseCode == AckResponseCode.Success)
                UpdateMapUsers(CentralAppServerRegister, UpdateUserCharacterMessage.UpdateType.Add);
        }
        #endregion

        #region Connect to chat server
        public void OnChatServerConnected()
        {
            if (LogInfo)
                Debug.Log("Connected to chat server");
            UpdateMapUsers(ChatNetworkManager.Client, UpdateUserCharacterMessage.UpdateType.Add);
        }

        public void OnChatMessageReceive(ChatMessage message)
        {
            ReadChatMessage(message);
        }

        public void OnUpdateMapUser(UpdateUserCharacterMessage message)
        {
            int socialId;
            PartyData party;
            GuildData guild;
            switch (message.type)
            {
                case UpdateUserCharacterMessage.UpdateType.Add:
                    if (!usersById.ContainsKey(message.CharacterId))
                        usersById.Add(message.CharacterId, message.data);
                    break;
                case UpdateUserCharacterMessage.UpdateType.Remove:
                    usersById.Remove(message.CharacterId);
                    break;
                case UpdateUserCharacterMessage.UpdateType.Online:
                    if (usersById.ContainsKey(message.CharacterId))
                    {
                        socialId = message.data.partyId;
                        if (socialId > 0 && parties.TryGetValue(socialId, out party))
                        {
                            party.UpdateMember(message.data.ToSocialCharacterData());
                            party.NotifyOnlineMember(message.CharacterId);
                            parties[socialId] = party;
                        }
                        socialId = message.data.guildId;
                        if (socialId > 0 && guilds.TryGetValue(socialId, out guild))
                        {
                            guild.UpdateMember(message.data.ToSocialCharacterData());
                            guild.NotifyOnlineMember(message.CharacterId);
                            guilds[socialId] = guild;
                        }
                        usersById[message.CharacterId] = message.data;
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
                        if (playerCharactersById.TryGetValue(message.CharacterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.PartyId = message.id;
                            SendCreatePartyToClient(playerCharacterEntity.ConnectionId, party);
                            SendAddPartyMembersToClient(playerCharacterEntity.ConnectionId, party);
                        }
                        SendAddPartyMemberToClients(party, message.CharacterId, message.data.id, message.data.dataId, message.data.level);
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (playerCharactersById.TryGetValue(message.CharacterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.ClearParty();
                            SendPartyTerminateToClient(playerCharacterEntity.ConnectionId, message.id);
                        }
                        SendRemovePartyMemberToClients(party, message.CharacterId);
                        break;
                }
            }
        }

        public void OnUpdateParty(UpdatePartyMessage message)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            PartyData party;
            if (parties.TryGetValue(message.id, out party))
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        party.SetLeader(message.characterId);
                        parties[message.id] = party;
                        SendChangePartyLeaderToClients(party);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        party.Setting(message.shareExp, message.shareItem);
                        parties[message.id] = party;
                        SendPartySettingToClients(party);
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        foreach (string memberId in party.GetMemberIds())
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                            {
                                playerCharacterEntity.ClearParty();
                                SendPartyTerminateToClient(playerCharacterEntity.ConnectionId, message.id);
                            }
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
                        if (playerCharactersById.TryGetValue(message.CharacterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.GuildId = message.id;
                            playerCharacterEntity.GuildName = guild.guildName;
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                            SendCreateGuildToClient(playerCharacterEntity.ConnectionId, guild);
                            SendAddGuildMembersToClient(playerCharacterEntity.ConnectionId, guild);
                        }
                        SendAddGuildMemberToClients(guild, message.CharacterId, message.data.id, message.data.dataId, message.data.level);
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (playerCharactersById.TryGetValue(message.CharacterId, out playerCharacterEntity))
                        {
                            playerCharacterEntity.ClearGuild();
                            SendGuildTerminateToClient(playerCharacterEntity.ConnectionId, message.id);
                        }
                        SendRemoveGuildMemberToClients(guild, message.CharacterId);
                        break;
                }
            }
        }

        public void OnUpdateGuild(UpdateGuildMessage message)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            GuildData guild;
            if (guilds.TryGetValue(message.id, out guild))
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        guild.SetLeader(message.characterId);
                        guilds[message.id] = guild;
                        if (TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                        SendChangeGuildLeaderToClients(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        guild.guildMessage = message.guildMessage;
                        guilds[message.id] = guild;
                        SendSetGuildMessageToClients(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        guild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        guilds[message.id] = guild;
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                                playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                        }
                        SendSetGuildRoleToClients(guild, message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        guild.SetMemberRole(message.characterId, message.guildRole);
                        guilds[message.id] = guild;
                        if (TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                        SendSetGuildMemberRoleToClients(guild, message.characterId, message.guildRole);
                        break;
                    case UpdateGuildMessage.UpdateType.SetSkillLevel:
                        guild.SetSkillLevel(message.dataId, message.level);
                        guilds[message.id] = guild;
                        SendSetGuildSkillLevelToClients(guild, message.dataId);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGold:
                        guild.gold = message.gold;
                        guilds[message.id] = guild;
                        SendSetGuildGoldToClients(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.LevelExpSkillPoint:
                        guild.level = message.level;
                        guild.exp = message.exp;
                        guild.skillPoint = message.skillPoint;
                        guilds[message.id] = guild;
                        SendGuildLevelExpSkillPointToClients(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                            {
                                playerCharacterEntity.ClearGuild();
                                SendGuildTerminateToClient(playerCharacterEntity.ConnectionId, message.id);
                            }
                        }
                        guilds.Remove(message.id);
                        break;
                }
            }
        }
        #endregion

        #region Update map user functions
        private void UpdateMapUsers(TransportHandler transportHandler, UpdateUserCharacterMessage.UpdateType updateType)
        {
            foreach (UserCharacterData user in usersById.Values)
            {
                UpdateMapUser(transportHandler, updateType, user);
            }
        }

        private void UpdateMapUser(TransportHandler transportHandler, UpdateUserCharacterMessage.UpdateType updateType, UserCharacterData userData)
        {
            UpdateUserCharacterMessage updateMapUserMessage = new UpdateUserCharacterMessage();
            updateMapUserMessage.type = updateType;
            updateMapUserMessage.data = userData;
            transportHandler.ClientSendPacket(DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, updateMapUserMessage.Serialize);
        }
        #endregion
    }
}
