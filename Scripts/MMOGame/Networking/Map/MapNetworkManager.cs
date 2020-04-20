using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public sealed partial class MapNetworkManager : BaseGameNetworkManager, IAppServer
    {
        public const float TERMINATE_INSTANCE_DELAY = 30f;  // Close instance when no clients connected within 30 seconds

        public struct PendingSpawnPlayerCharacter
        {
            public long connectionId;
            public string userId;
            public string accessToken;
            public string selectCharacterId;
        }
        private readonly List<PendingSpawnPlayerCharacter> pendingSpawnPlayerCharacters = new List<PendingSpawnPlayerCharacter>();

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
            get { return centralTransportFactory; }
        }

        public CentralAppServerRegister CentralAppServerRegister { get; private set; }

        public ChatNetworkManager ChatNetworkManager { get; private set; }
        
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
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
        private readonly Dictionary<StorageId, List<CharacterItem>> storageItems = new Dictionary<StorageId, List<CharacterItem>>();
        private readonly Dictionary<StorageId, HashSet<uint>> usingStorageCharacters = new Dictionary<StorageId, HashSet<uint>>();
        // Database operations
        private readonly HashSet<StorageId> loadingStorageIds = new HashSet<StorageId>();
        private readonly HashSet<int> loadingPartyIds = new HashSet<int>();
        private readonly HashSet<int> loadingGuildIds = new HashSet<int>();
        private readonly HashSet<string> savingCharacters = new HashSet<string>();
        private readonly HashSet<string> savingBuildings = new HashSet<string>();

        protected override void Awake()
        {
            base.Awake();
            if (useWebSocket)
            {
                if (centralTransportFactory == null || !centralTransportFactory.CanUseWithWebGL)
                    centralTransportFactory = gameObject.AddComponent<WebSocketTransportFactory>();
            }
            else
            {
                if (centralTransportFactory == null)
                    centralTransportFactory = gameObject.AddComponent<LiteNetLibTransportFactory>();
            }
            CentralAppServerRegister = new CentralAppServerRegister(CentralTransportFactory.Build(), this);
            CentralAppServerRegister.onAppServerRegistered = OnAppServerRegistered;
            CentralAppServerRegister.RegisterMessage(MMOMessageTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
            this.InvokeInstanceDevExtMethods("OnInitCentralAppServerRegister");
            ChatNetworkManager = gameObject.AddComponent<ChatNetworkManager>();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            float tempUnscaledTime = Time.unscaledTime;
            if (IsServer)
            {
                CentralAppServerRegister.Update();
                if (tempUnscaledTime - lastSaveTime > autoSaveDuration)
                {
                    lastSaveTime = tempUnscaledTime;
                    SaveCharactersRoutine();
                    if (!IsInstanceMap())
                    {
                        // Don't save building if it's instance map
                        SaveBuildingsRoutine();
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

                if (pendingSpawnPlayerCharacters.Count > 0 && IsReadyToInstantiateObjects())
                {
                    // Spawn pending player characters
                    foreach (PendingSpawnPlayerCharacter spawnPlayerCharacter in pendingSpawnPlayerCharacters)
                    {
                        SetPlayerReady(spawnPlayerCharacter.connectionId, spawnPlayerCharacter.userId, spawnPlayerCharacter.accessToken, spawnPlayerCharacter.selectCharacterId);
                    }
                    pendingSpawnPlayerCharacters.Clear();
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
            storageItems.Clear();
            usingStorageCharacters.Clear();
            loadingStorageIds.Clear();
            loadingPartyIds.Clear();
            loadingGuildIds.Clear();
            savingCharacters.Clear();
            savingBuildings.Clear();
        }

        protected override void UpdateOnlineCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            base.UpdateOnlineCharacter(playerCharacterEntity);

            UserCharacterData tempUserData;
            if (ChatNetworkManager.IsClientConnected && usersById.TryGetValue(playerCharacterEntity.Id, out tempUserData))
            {
                tempUserData.dataId = playerCharacterEntity.DataId;
                tempUserData.level = playerCharacterEntity.Level;
                tempUserData.currentHp = playerCharacterEntity.CurrentHp;
                tempUserData.maxHp = playerCharacterEntity.MaxHp;
                tempUserData.currentMp = playerCharacterEntity.CurrentMp;
                tempUserData.maxMp = playerCharacterEntity.MaxMp;
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
                    if (playerCharacter == null) continue;
                    DbServiceClient.UpdateCharacter(new UpdateCharacterReq()
                    {
                        CharacterData = playerCharacter.CloneTo(new PlayerCharacterData()).ToByteString()
                    });
                }
                string sceneName = Assets.onlineScene.SceneName;
                foreach (BuildingEntity buildingEntity in buildingEntities.Values)
                {
                    if (buildingEntity == null) continue;
                    DbServiceClient.UpdateBuilding(new UpdateBuildingReq()
                    {
                        MapName = sceneName,
                        BuildingData = buildingEntity.CloneTo(new BuildingSaveData()).ToByteString()
                    });
                }
            }
            base.OnDestroy();
        }

        public override void RegisterPlayerCharacter(BasePlayerCharacterEntity playerCharacterEntity)
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
                userData.maxHp = playerCharacterEntity.MaxHp;
                userData.currentMp = playerCharacterEntity.CurrentMp;
                userData.maxMp = playerCharacterEntity.MaxMp;
                usersById.Add(userData.id, userData);
                // Add map user to central server and chat server
                UpdateMapUser(CentralAppServerRegister, UpdateUserCharacterMessage.UpdateType.Add, userData);
                if (ChatNetworkManager.IsClientConnected)
                    UpdateMapUser(ChatNetworkManager.Client, UpdateUserCharacterMessage.UpdateType.Add, userData);
            }
            base.RegisterPlayerCharacter(playerCharacterEntity);
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
            OnPeerDisconnectedRoutine(connectionId, disconnectInfo);
        }

        private async void OnPeerDisconnectedRoutine(long connectionId, DisconnectInfo disconnectInfo)
        {
            // Save player character data
            BasePlayerCharacterEntity playerCharacterEntity;
            if (playerCharacters.TryGetValue(connectionId, out playerCharacterEntity))
            {
                PlayerCharacterData saveCharacterData = playerCharacterEntity.CloneTo(new PlayerCharacterData());
                while (savingCharacters.Contains(saveCharacterData.Id))
                {
                    await Task.Yield();
                }
                await SaveCharacterRoutine(saveCharacterData, playerCharacterEntity.UserId);
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

        protected override async Task PreSpawnEntities()
        {
            // Spawn buildings
            if (!IsInstanceMap())
            {
                // Load buildings
                // Don't load buildings if it's instance map
                ReadBuildingsResp resp = await DbServiceClient.ReadBuildingsAsync(new ReadBuildingsReq()
                {
                    MapName = Assets.onlineScene.SceneName
                });
                HashSet<StorageId> storageIds = new HashSet<StorageId>();
                List<BuildingSaveData> buildings = resp.List.MakeListFromRepeatedByteString<BuildingSaveData>();
                BuildingEntity buildingEntity;
                foreach (BuildingSaveData building in buildings)
                {
                    buildingEntity = CreateBuildingEntity(building, true);
                    if (buildingEntity is StorageEntity)
                        storageIds.Add(new StorageId(StorageType.Building, (buildingEntity as StorageEntity).Id));
                }
                // Load building storage
                foreach (StorageId storageId in storageIds)
                {
                    await LoadStorageRoutine(storageId);
                }
                // Wait until all building storage loaded
                while (loadingStorageIds.Count > 0)
                {
                    await Task.Yield();
                }
            }
        }

        protected override async Task PostSpawnEntities()
        {
            await Task.Yield();
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

            SetPlayerReady(connectionId, reader.GetString(), reader.GetString(), reader.GetString());
        }

        private void SetPlayerReady(long connectionId, string userId, string accessToken, string selectCharacterId)
        {
            if (!IsReadyToInstantiateObjects())
            {
                if (LogError)
                    Logging.LogError(LogTag, "Not ready to spawn player: " + userId);
                // Add to pending list to spawn player later when map server is ready to instantiate object
                pendingSpawnPlayerCharacters.Add(new PendingSpawnPlayerCharacter()
                {
                    connectionId = connectionId,
                    userId = userId,
                    accessToken = accessToken,
                    selectCharacterId = selectCharacterId
                });
                return;
            }

            if (playerCharacters.ContainsKey(connectionId))
            {
                if (LogError)
                    Logging.LogError(LogTag, "User trying to hack: " + userId);
                Transport.ServerDisconnect(connectionId);
                return;
            }

            LiteNetLibPlayer player = GetPlayer(connectionId);
            if (player.IsReady)
                return;

            player.IsReady = true;

            SetPlayerReadyRoutine(connectionId, userId, accessToken, selectCharacterId);
        }

        private async void SetPlayerReadyRoutine(long connectionId, string userId, string accessToken, string selectCharacterId)
        {
            // Validate access token
            ValidateAccessTokenResp validateAccessTokenResp = await DbServiceClient.ValidateAccessTokenAsync(new ValidateAccessTokenReq()
            {
                UserId = userId,
                AccessToken = accessToken
            });
            if (!validateAccessTokenResp.IsPass)
            {
                if (LogError)
                    Logging.LogError(LogTag, "Invalid access token for user: " + userId);
                Transport.ServerDisconnect(connectionId);
            }
            else
            {
                ReadCharacterResp readCharacterResp = await DbServiceClient.ReadCharacterAsync(new ReadCharacterReq()
                {
                    UserId = userId,
                    CharacterId = selectCharacterId,
                    WithEquipWeapons = true,
                    WithAttributes = true,
                    WithSkills = true,
                    WithSkillUsages = true,
                    WithBuffs = true,
                    WithEquipItems = true,
                    WithNonEquipItems = true,
                    WithSummons = true,
                    WithHotkeys = true,
                    WithQuests = true
                });
                PlayerCharacterData playerCharacterData = readCharacterResp.CharacterData.FromByteString<PlayerCharacterData>();
                // If data is empty / cannot find character, disconnect user
                if (playerCharacterData == null)
                {
                    if (LogError)
                        Logging.LogError(LogTag, "Cannot find select character: " + selectCharacterId + " for user: " + userId);
                    Transport.ServerDisconnect(connectionId);
                }
                else
                {
                    BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
                    // If it is not allow this character data, disconnect user
                    if (entityPrefab == null)
                    {
                        if (LogError)
                            Logging.LogError(LogTag, "Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                        Transport.ServerDisconnect(connectionId);
                    }
                    else
                    {
                        // Prepare saving location for this character
                        string savingCurrentMapName = playerCharacterData.CurrentMapName;
                        Vector3 savingCurrentPosition = playerCharacterData.CurrentPosition;

                        // Move character to map info → start position
                        if (IsInstanceMap())
                            playerCharacterData.CurrentPosition = CurrentMapInfo.StartPosition;

                        // Spawn character entity and set its data
                        GameObject spawnObj = Instantiate(entityPrefab.gameObject, playerCharacterData.CurrentPosition, Quaternion.identity);
                        BasePlayerCharacterEntity playerCharacterEntity = spawnObj.GetComponent<BasePlayerCharacterEntity>();
                        playerCharacterData.CloneTo(playerCharacterEntity);
                        Assets.NetworkSpawn(spawnObj, 0, connectionId);

                        // Set currencies
                        // Gold
                        GoldResp getGoldResp = await DbServiceClient.GetGoldAsync(new GetGoldReq()
                        {
                            UserId = userId
                        });
                        playerCharacterEntity.UserGold = getGoldResp.Gold;
                        // Cash
                        CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                        {
                            UserId = userId
                        });
                        playerCharacterEntity.UserCash = getCashResp.Cash;

                        // Prepare saving location for this character
                        if (IsInstanceMap())
                            instanceMapCurrentLocations.Add(playerCharacterEntity.ObjectId, new KeyValuePair<string, Vector3>(savingCurrentMapName, savingCurrentPosition));

                        // Set user Id
                        playerCharacterEntity.UserId = userId;

                        // Load user level
                        GetUserLevelResp getUserLevelResp = await DbServiceClient.GetUserLevelAsync(new GetUserLevelReq()
                        {
                            UserId = userId
                        });
                        playerCharacterEntity.UserLevel = (byte)getUserLevelResp.UserLevel;

                        // Load storage
                        await LoadStorageRoutine(new StorageId(StorageType.Player, userId));

                        // Load party data, if this map-server does not have party data
                        if (playerCharacterEntity.PartyId > 0)
                        {
                            if (!parties.ContainsKey(playerCharacterEntity.PartyId))
                                await LoadPartyRoutine(playerCharacterEntity.PartyId);
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
                                await LoadGuildRoutine(playerCharacterEntity.GuildId);
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

                        // Summon saved mount entity
                        if (GameInstance.VehicleEntities.ContainsKey(playerCharacterData.MountDataId))
                            playerCharacterEntity.Mount(GameInstance.VehicleEntities[playerCharacterData.MountDataId]);

                        // Force make caches, to calculate current stats to fill empty slots items
                        playerCharacterEntity.ForceMakeCaches();
                        playerCharacterEntity.FillEmptySlots();

                        // Notify clients that this character is spawn or dead
                        if (!playerCharacterEntity.IsDead())
                            playerCharacterEntity.RequestOnRespawn();
                        else
                            playerCharacterEntity.RequestOnDead();

                        // Register player character entity to the server
                        RegisterPlayerCharacter(playerCharacterEntity);

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
            if (message.channel == ChatChannel.System)
            {
                BasePlayerCharacterEntity playerCharacter;
                // TODO: Don't use fixed user level
                if (!string.IsNullOrEmpty(message.sender) &&
                    TryGetPlayerCharacterByName(message.sender, out playerCharacter) &&
                    playerCharacter.UserLevel > 0)
                {
                    // Send chat message to chat server, for MMO mode chat message handling by chat server
                    if (ChatNetworkManager.IsClientConnected)
                    {
                        ChatNetworkManager.SendEnterChat(null, MMOMessageTypes.Chat, message.channel, message.message, message.sender, message.receiver, message.channelId);
                    }
                }
                return;
            }
            // Send chat message to chat server, for MMO mode chat message handling by chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendEnterChat(null, MMOMessageTypes.Chat, message.channel, message.message, message.sender, message.receiver, message.channelId);
            }
        }

        protected override void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestCashShopInfoRoutine(messageHandler);
        }

        private async void HandleRequestCashShopInfoRoutine(LiteNetLibMessageHandler messageHandler)
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
            {
                // Canot find user
                error = ResponseCashShopInfoMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = userData.userId
                });
                cash = getCashResp.Cash;
                // Set cash shop item ids
                cashShopItemIds.AddRange(GameInstance.CashShopItems.Keys);
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
            HandleRequestCashShopBuyRoutine(messageHandler);
        }

        private async void HandleRequestCashShopBuyRoutine(LiteNetLibMessageHandler messageHandler)
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
            {
                // Canot find user
                error = ResponseCashShopBuyMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = userData.userId
                });
                cash = getCashResp.Cash;
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                {
                    // Cannot find item
                    error = ResponseCashShopBuyMessage.Error.ItemNotFound;
                }
                else if (cash < cashShopItem.sellPrice)
                {
                    // Not enough cash
                    error = ResponseCashShopBuyMessage.Error.NotEnoughCash;
                }
                else if (playerCharacter.IncreasingItemsWillOverwhelming(cashShopItem.receiveItems))
                {
                    // Cannot carry all rewards
                    error = ResponseCashShopBuyMessage.Error.CannotCarryAllRewards;
                }
                else
                {
                    // Decrease cash amount
                    cash -= cashShopItem.sellPrice;
                    await DbServiceClient.UpdateCashAsync(new UpdateCashReq()
                    {
                        UserId = userData.userId,
                        Amount = cash
                    });
                    playerCharacter.UserCash = cash;
                    // Increase character gold
                    playerCharacter.Gold += cashShopItem.receiveGold;
                    // Increase character item
                    foreach (ItemAmount receiveItem in cashShopItem.receiveItems)
                    {
                        if (receiveItem.item == null || receiveItem.amount <= 0) continue;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
                    }
                    playerCharacter.FillEmptySlots();
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
            HandleRequestCashPackageInfoRoutine(messageHandler);
        }

        private async void HandleRequestCashPackageInfoRoutine(LiteNetLibMessageHandler messageHandler)
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
            {
                // Canot find user
                error = ResponseCashPackageInfoMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = userData.userId
                });
                cash = getCashResp.Cash;
                // Set cash package ids
                cashPackageIds.AddRange(GameInstance.CashPackages.Keys);
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
            HandleRequestCashPackageBuyValidationRoutine(messageHandler);
        }

        private async void HandleRequestCashPackageBuyValidationRoutine(LiteNetLibMessageHandler messageHandler)
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
            {
                // Canot find user
                error = ResponseCashPackageBuyValidationMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = userData.userId
                });
                cash = getCashResp.Cash;
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                {
                    // Cannot find package
                    error = ResponseCashPackageBuyValidationMessage.Error.PackageNotFound;
                }
                else
                {
                    // Increase cash amount
                    cash += cashPackage.cashAmount;
                    await DbServiceClient.UpdateCashAsync(new UpdateCashReq()
                    {
                        UserId = userData.userId,
                        Amount = cash
                    });
                    playerCharacter.UserCash = cash;
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
                                Logging.Log(LogTag, "Register map server: " + peerInfo.extra);
                            mapServerConnectionIdsBySceneName[peerInfo.extra] = peerInfo;
                        }
                        break;
                    case CentralServerPeerType.InstanceMapServer:
                        if (!string.IsNullOrEmpty(peerInfo.extra))
                        {
                            if (LogInfo)
                                Logging.Log(LogTag, "Register instance map server: " + peerInfo.extra);
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
                                    WarpCharacterToInstanceRoutine(warpingCharacterEntity, peerInfo.extra);
                                }
                            }
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        if (!ChatNetworkManager.IsClientConnected)
                        {
                            if (LogInfo)
                                Logging.Log(LogTag, "Connecting to chat server");
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
                Logging.Log(LogTag, "Connected to chat server");
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
                    if (!usersById.ContainsKey(message.data.id))
                        usersById.Add(message.data.id, message.data);
                    break;
                case UpdateUserCharacterMessage.UpdateType.Remove:
                    usersById.Remove(message.data.id);
                    break;
                case UpdateUserCharacterMessage.UpdateType.Online:
                    if (usersById.ContainsKey(message.data.id))
                    {
                        NotifyOnlineCharacter(message.data.id);
                        socialId = message.data.partyId;
                        if (socialId > 0 && parties.TryGetValue(socialId, out party))
                        {
                            party.UpdateMember(message.data.ToSocialCharacterData());
                            parties[socialId] = party;
                        }
                        socialId = message.data.guildId;
                        if (socialId > 0 && guilds.TryGetValue(socialId, out guild))
                        {
                            guild.UpdateMember(message.data.ToSocialCharacterData());
                            guilds[socialId] = guild;
                        }
                        usersById[message.data.id] = message.data;
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
                        if (playerCharactersById.TryGetValue(message.data.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.PartyId = message.id;
                            SendCreatePartyToClient(playerCharacterEntity.ConnectionId, party);
                            SendAddPartyMembersToClient(playerCharacterEntity.ConnectionId, party);
                        }
                        SendAddPartyMemberToClients(party, message.data.id, message.data.characterName, message.data.dataId, message.data.level);
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (playerCharactersById.TryGetValue(message.data.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.ClearParty();
                            SendPartyTerminateToClient(playerCharacterEntity.ConnectionId, message.id);
                        }
                        SendRemovePartyMemberToClients(party, message.data.id);
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
                        if (playerCharactersById.TryGetValue(message.data.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.GuildId = message.id;
                            playerCharacterEntity.GuildName = guild.guildName;
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                            SendCreateGuildToClient(playerCharacterEntity.ConnectionId, guild);
                            SendAddGuildMembersToClient(playerCharacterEntity.ConnectionId, guild);
                        }
                        SendAddGuildMemberToClients(guild, message.data.id, message.data.characterName, message.data.dataId, message.data.level);
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (playerCharactersById.TryGetValue(message.data.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.ClearGuild();
                            SendGuildTerminateToClient(playerCharacterEntity.ConnectionId, message.id);
                        }
                        SendRemoveGuildMemberToClients(guild, message.data.id);
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
        private void UpdateMapUsers(LiteNetLibClient transportHandler, UpdateUserCharacterMessage.UpdateType updateType)
        {
            foreach (UserCharacterData user in usersById.Values)
            {
                UpdateMapUser(transportHandler, updateType, user);
            }
        }

        private void UpdateMapUser(LiteNetLibClient transportHandler, UpdateUserCharacterMessage.UpdateType updateType, UserCharacterData userData)
        {
            UpdateUserCharacterMessage updateMapUserMessage = new UpdateUserCharacterMessage();
            updateMapUserMessage.type = updateType;
            updateMapUserMessage.data = userData;
            transportHandler.SendPacket(DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, updateMapUserMessage.Serialize);
        }
        #endregion
    }
}
