using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(-894)]
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

        public struct InstanceMapWarpingLocation
        {
            public string mapName;
            public Vector3 position;
            public bool overrideRotation;
            public Vector3 rotation;
        }

        /// <summary>
        /// If this is not empty it mean this is temporary instance map
        /// So it won't have to save current map, current position to database
        /// </summary>
        public string MapInstanceId { get; set; }
        public Vector3 MapInstanceWarpToPosition { get; set; }
        public bool MapInstanceWarpOverrideRotation { get; set; }
        public Vector3 MapInstanceWarpToRotation { get; set; }

        [Header("Central Network Connection")]
        public string clusterServerAddress = "127.0.0.1";
        public int clusterServerPort = 6010;
        public string machineAddress = "127.0.0.1";

        [Header("Database")]
        public float autoSaveDuration = 2f;

        [Header("Map Spawn")]
        public int mapSpawnMillisecondsTimeout = 0;

        [Header("Player Disconnection")]
        public int playerCharacterDespawnMillisecondsDelay = 5000;

        private float terminatingTime;

#if UNITY_EDITOR || UNITY_SERVER
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public ClusterClient ClusterClient { get; private set; }
#endif

        public string ClusterServerAddress { get { return clusterServerAddress; } }
        public int ClusterServerPort { get { return clusterServerPort; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppExtra
        {
            get
            {
                if (IsInstanceMap())
                    return MapInstanceId;
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
#if UNITY_EDITOR || UNITY_SERVER
        private readonly List<PendingSpawnPlayerCharacter> pendingSpawnPlayerCharacters = new List<PendingSpawnPlayerCharacter>();
        private readonly ConcurrentDictionary<uint, KeyValuePair<string, Vector3>> instanceMapCurrentLocations = new ConcurrentDictionary<uint, KeyValuePair<string, Vector3>>();
        private readonly ConcurrentDictionary<string, CentralServerPeerInfo> mapServerConnectionIdsBySceneName = new ConcurrentDictionary<string, CentralServerPeerInfo>();
        private readonly ConcurrentDictionary<string, CentralServerPeerInfo> instanceMapServerConnectionIdsByInstanceId = new ConcurrentDictionary<string, CentralServerPeerInfo>();
        private readonly ConcurrentDictionary<string, HashSet<uint>> instanceMapWarpingCharactersByInstanceId = new ConcurrentDictionary<string, HashSet<uint>>();
        private readonly ConcurrentDictionary<string, InstanceMapWarpingLocation> instanceMapWarpingLocations = new ConcurrentDictionary<string, InstanceMapWarpingLocation>();
        private readonly ConcurrentDictionary<string, SocialCharacterData> usersById = new ConcurrentDictionary<string, SocialCharacterData>();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> despawningPlayerCharacterCancellations = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly ConcurrentDictionary<string, BasePlayerCharacterEntity> despawningPlayerCharacterEntities = new ConcurrentDictionary<string, BasePlayerCharacterEntity>();
        private readonly ConcurrentDictionary<string, long> connectionsByUserId = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<long, string> accessTokensByConnectionId = new ConcurrentDictionary<long, string>();
        // Database operations
        private readonly HashSet<StorageId> loadingStorageIds = new HashSet<StorageId>();
        private readonly HashSet<int> loadingPartyIds = new HashSet<int>();
        private readonly HashSet<int> loadingGuildIds = new HashSet<int>();
        internal readonly HashSet<string> savingCharacters = new HashSet<string>();
        internal readonly HashSet<string> savingBuildings = new HashSet<string>();
#endif

        protected override void Awake()
        {
            // Server Handlers
            ServerMailHandlers = gameObject.GetOrAddComponent<IServerMailHandlers, MMOServerMailHandlers>();
            ServerUserHandlers = gameObject.GetOrAddComponent<IServerUserHandlers, MMOServerUserHandlers>();
            ServerBuildingHandlers = gameObject.GetOrAddComponent<IServerBuildingHandlers, DefaultServerBuildingHandlers>();
            ServerCharacterHandlers = gameObject.GetOrAddComponent<IServerCharacterHandlers, DefaultServerCharacterHandlers>();
            ServerGameMessageHandlers = gameObject.GetOrAddComponent<IServerGameMessageHandlers, DefaultServerGameMessageHandlers>();
            ServerStorageHandlers = gameObject.GetOrAddComponent<IServerStorageHandlers, MMOServerStorageHandlers>();
            ServerPartyHandlers = gameObject.GetOrAddComponent<IServerPartyHandlers, DefaultServerPartyHandlers>();
            ServerGuildHandlers = gameObject.GetOrAddComponent<IServerGuildHandlers, MMOServerGuildHandlers>();
            ServerChatHandlers = gameObject.GetOrAddComponent<IServerChatHandlers, DefaultServerChatHandlers>();
            // Server Message Handlers
            ServerCashShopMessageHandlers = gameObject.GetOrAddComponent<IServerCashShopMessageHandlers, MMOServerCashShopMessageHandlers>();
            ServerMailMessageHandlers = gameObject.GetOrAddComponent<IServerMailMessageHandlers, MMOServerMailMessageHandlers>();
            ServerStorageMessageHandlers = gameObject.GetOrAddComponent<IServerStorageMessageHandlers, MMOServerStorageMessageHandlers>();
            ServerCharacterMessageHandlers = gameObject.GetOrAddComponent<IServerCharacterMessageHandlers, DefaultServerCharacterMessageHandlers>();
            ServerInventoryMessageHandlers = gameObject.GetOrAddComponent<IServerInventoryMessageHandlers, DefaultServerInventoryMessageHandlers>();
            ServerPartyMessageHandlers = gameObject.GetOrAddComponent<IServerPartyMessageHandlers, MMOServerPartyMessageHandlers>();
            ServerGuildMessageHandlers = gameObject.GetOrAddComponent<IServerGuildMessageHandlers, MMOServerGuildMessageHandlers>();
            ServerGachaMessageHandlers = gameObject.GetOrAddComponent<IServerGachaMessageHandlers, MMOServerGachaMessageHandlers>();
            ServerFriendMessageHandlers = gameObject.GetOrAddComponent<IServerFriendMessageHandlers, MMOServerFriendMessageHandlers>();
            ServerBankMessageHandlers = gameObject.GetOrAddComponent<IServerBankMessageHandlers, MMOServerBankMessageHandlers>();
            ServerOnlineCharacterMessageHandlers = gameObject.GetOrAddComponent<IServerOnlineCharacterMessageHandlers, DefaultServerOnlineCharacterMessageHandlers>();
            // Client handlers
            ClientCashShopHandlers = gameObject.GetOrAddComponent<IClientCashShopHandlers, DefaultClientCashShopHandlers>();
            ClientMailHandlers = gameObject.GetOrAddComponent<IClientMailHandlers, DefaultClientMailHandlers>();
            ClientStorageHandlers = gameObject.GetOrAddComponent<IClientStorageHandlers, DefaultClientStorageHandlers>();
            ClientCharacterHandlers = gameObject.GetOrAddComponent<IClientCharacterHandlers, DefaultClientCharacterHandlers>();
            ClientInventoryHandlers = gameObject.GetOrAddComponent<IClientInventoryHandlers, DefaultClientInventoryHandlers>();
            ClientPartyHandlers = gameObject.GetOrAddComponent<IClientPartyHandlers, DefaultClientPartyHandlers>();
            ClientGuildHandlers = gameObject.GetOrAddComponent<IClientGuildHandlers, DefaultClientGuildHandlers>();
            ClientGachaHandlers = gameObject.GetOrAddComponent<IClientGachaHandlers, DefaultClientGachaHandlers>();
            ClientFriendHandlers = gameObject.GetOrAddComponent<IClientFriendHandlers, DefaultClientFriendHandlers>();
            ClientBankHandlers = gameObject.GetOrAddComponent<IClientBankHandlers, DefaultClientBankHandlers>();
            ClientOnlineCharacterHandlers = gameObject.GetOrAddComponent<IClientOnlineCharacterHandlers, DefaultClientOnlineCharacterHandlers>();
            ClientChatHandlers = gameObject.GetOrAddComponent<IClientChatHandlers, DefaultClientChatHandlers>();
            ClientGameMessageHandlers = gameObject.GetOrAddComponent<IClientGameMessageHandlers, DefaultClientGameMessageHandlers>();
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
#if UNITY_EDITOR || UNITY_SERVER
            // Cluster client which will be used by map server to connect to cluster server
            ClusterClient = new ClusterClient(this);
            ClusterClient.onResponseAppServerRegister = OnResponseAppServerRegister;
            ClusterClient.onResponseAppServerAddress = OnResponseAppServerAddress;
            ClusterClient.RegisterResponseHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap);
            ClusterClient.RegisterMessageHandler(MMOMessageTypes.Chat, HandleChat);
            ClusterClient.RegisterMessageHandler(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            ClusterClient.RegisterMessageHandler(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMember);
            ClusterClient.RegisterMessageHandler(MMOMessageTypes.UpdateParty, HandleUpdateParty);
            ClusterClient.RegisterMessageHandler(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMember);
            ClusterClient.RegisterMessageHandler(MMOMessageTypes.UpdateGuild, HandleUpdateGuild);
#endif
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
#if UNITY_EDITOR || UNITY_SERVER
            float tempTime = Time.fixedTime;
            if (IsServer)
            {
                ClusterClient.Update();

                if (tempTime - lastSaveTime > autoSaveDuration)
                {
                    lastSaveTime = tempTime;
                    SaveAllCharacters().Forget();
                    if (!IsInstanceMap())
                    {
                        // Don't save building if it's instance map
                        SaveAllBuildings().Forget();
                    }
                }

                if (IsInstanceMap())
                {
                    // Quitting application when no players
                    if (Players.Count > 0)
                        terminatingTime = tempTime;
                    else if (tempTime - terminatingTime >= TERMINATE_INSTANCE_DELAY)
                        Application.Quit();
                }

                if (pendingSpawnPlayerCharacters.Count > 0 && IsReadyToInstantiateObjects())
                {
                    // Spawn pending player characters
                    LiteNetLibPlayer player;
                    foreach (PendingSpawnPlayerCharacter spawnPlayerCharacter in pendingSpawnPlayerCharacters)
                    {
                        if (!Players.TryGetValue(spawnPlayerCharacter.connectionId, out player))
                            continue;
                        player.IsReady = true;
                        SetPlayerReadyRoutine(spawnPlayerCharacter.connectionId, spawnPlayerCharacter.userId, spawnPlayerCharacter.accessToken, spawnPlayerCharacter.selectCharacterId).Forget();
                    }
                    pendingSpawnPlayerCharacters.Clear();
                }
            }
#endif
        }

        protected override void Clean()
        {
            base.Clean();
#if UNITY_EDITOR || UNITY_SERVER
            instanceMapCurrentLocations.Clear();
            mapServerConnectionIdsBySceneName.Clear();
            instanceMapServerConnectionIdsByInstanceId.Clear();
            instanceMapWarpingCharactersByInstanceId.Clear();
            instanceMapWarpingLocations.Clear();
            usersById.Clear();
            loadingStorageIds.Clear();
            loadingPartyIds.Clear();
            loadingGuildIds.Clear();
            savingCharacters.Clear();
            savingBuildings.Clear();
            connectionsByUserId.Clear();
            accessTokensByConnectionId.Clear();
#endif
        }

        protected override void UpdateOnlineCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            base.UpdateOnlineCharacter(playerCharacterEntity);
#if UNITY_EDITOR || UNITY_SERVER
            SocialCharacterData tempUserData;
            if (ClusterClient.IsNetworkActive && usersById.TryGetValue(playerCharacterEntity.Id, out tempUserData))
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
                UpdateMapUser(ClusterClient, UpdateUserCharacterMessage.UpdateType.Online, tempUserData);
            }
#endif
        }

        protected async override void OnDestroy()
        {
            // Save immediately
#if UNITY_EDITOR || UNITY_SERVER
            if (IsServer)
            {
                foreach (BasePlayerCharacterEntity playerCharacter in ServerUserHandlers.GetPlayerCharacters())
                {
                    if (playerCharacter == null) continue;
                    await DbServiceClient.UpdateCharacterAsync(new UpdateCharacterReq()
                    {
                        CharacterData = playerCharacter.CloneTo(new PlayerCharacterData())
                    });
                }
                string mapName = CurrentMapInfo.Id;
                foreach (BuildingEntity buildingEntity in ServerBuildingHandlers.GetBuildings())
                {
                    if (buildingEntity == null) continue;
                    await DbServiceClient.UpdateBuildingAsync(new UpdateBuildingReq()
                    {
                        MapName = mapName,
                        BuildingData = buildingEntity.CloneTo(new BuildingSaveData())
                    });
                }
            }
#endif
            base.OnDestroy();
        }

#if UNITY_EDITOR || UNITY_SERVER
        public override void RegisterPlayerCharacter(long connectionId, BasePlayerCharacterEntity playerCharacterEntity)
        {
            // Set user data to map server
            if (!usersById.ContainsKey(playerCharacterEntity.Id))
            {
                SocialCharacterData userData = new SocialCharacterData();
                userData.userId = playerCharacterEntity.UserId;
                userData.id = playerCharacterEntity.Id;
                userData.characterName = playerCharacterEntity.CharacterName;
                userData.dataId = playerCharacterEntity.DataId;
                userData.level = playerCharacterEntity.Level;
                userData.currentHp = playerCharacterEntity.CurrentHp;
                userData.maxHp = playerCharacterEntity.MaxHp;
                userData.currentMp = playerCharacterEntity.CurrentMp;
                userData.maxMp = playerCharacterEntity.MaxMp;
                usersById.TryAdd(userData.id, userData);
                // Add map user to cluster server
                if (ClusterClient.IsNetworkActive)
                    UpdateMapUser(ClusterClient, UpdateUserCharacterMessage.UpdateType.Add, userData);
            }
            base.RegisterPlayerCharacter(connectionId, playerCharacterEntity);
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public override void UnregisterPlayerCharacter(long connectionId)
        {
            // Send remove character from map server
            BasePlayerCharacterEntity playerCharacter;
            SocialCharacterData userData;
            if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out playerCharacter) &&
                usersById.TryGetValue(playerCharacter.Id, out userData))
            {
                usersById.TryRemove(playerCharacter.Id, out _);
                // Remove map user from cluster server
                if (ClusterClient.IsNetworkActive)
                    UpdateMapUser(ClusterClient, UpdateUserCharacterMessage.UpdateType.Remove, userData);
            }
            base.UnregisterPlayerCharacter(connectionId);
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public override void RegisterUserId(long connectionId, string userId)
        {
            base.RegisterUserId(connectionId, userId);
            connectionsByUserId.TryRemove(userId, out _);
            connectionsByUserId.TryAdd(userId, connectionId);
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public override void UnregisterUserId(long connectionId)
        {
            string userId;
            if (ServerUserHandlers.TryGetUserId(connectionId, out userId))
                connectionsByUserId.TryRemove(userId, out _);
            base.UnregisterUserId(connectionId);
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(connectionId, disconnectInfo);
            OnPeerDisconnectedRoutine(connectionId, disconnectInfo).Forget();
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        private async UniTaskVoid OnPeerDisconnectedRoutine(long connectionId, DisconnectInfo disconnectInfo)
        {
            // Save player character data
            BasePlayerCharacterEntity playerCharacterEntity;
            if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out playerCharacterEntity))
            {
                playerCharacterEntity.SetOwnerClient(-1);
                playerCharacterEntity.StopMove();
                MovementState movementState = playerCharacterEntity.MovementState;
                movementState &= ~MovementState.Forward;
                movementState &= ~MovementState.Backward;
                movementState &= ~MovementState.Right;
                movementState &= ~MovementState.Left;
                playerCharacterEntity.KeyMovement(Vector3.zero, movementState);
                string id = playerCharacterEntity.Id;
                // Store despawning player character id, it will be used later if player not connect and continue playing the character
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                despawningPlayerCharacterCancellations.TryAdd(id, cancellationTokenSource);
                despawningPlayerCharacterEntities.TryAdd(id, playerCharacterEntity);
                // Save character immediately when player disconnect
                while (savingCharacters.Contains(id))
                {
                    await UniTask.Yield();
                }
                await SaveCharacter(playerCharacterEntity);
                // Saved, so can unregister character and user
                UnregisterPlayerCharacter(connectionId);
                UnregisterUserId(connectionId);
                accessTokensByConnectionId.TryRemove(connectionId, out _);
                try
                {
                    if (!IsInstanceMap())
                        await UniTask.Delay(playerCharacterDespawnMillisecondsDelay, true, PlayerLoopTiming.Update, cancellationTokenSource.Token);
                    // Save character again before despawned (it may attacked by other characters)
                    while (savingCharacters.Contains(id))
                    {
                        await UniTask.Yield(cancellationTokenSource.Token);
                    }
                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        await SaveCharacter(playerCharacterEntity);
                        playerCharacterEntity.NetworkDestroy();
                        despawningPlayerCharacterEntities.TryRemove(id, out _);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // Catch the cancellation
                }
                catch (System.Exception ex)
                {
                    // Other errors
                    Logging.LogException(LogTag, ex);
                }
                finally
                {
                    if (despawningPlayerCharacterCancellations.TryRemove(id, out cancellationTokenSource))
                        cancellationTokenSource.Dispose();
                }
            }
            else
            {
                UnregisterPlayerCharacter(connectionId);
                UnregisterUserId(connectionId);
                accessTokensByConnectionId.TryRemove(connectionId, out _);
            }
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public override void OnStopServer()
        {
            base.OnStopServer();
            ClusterClient.OnAppStop();
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        protected override async UniTask PreSpawnEntities()
        {
            // Spawn buildings
            if (!IsInstanceMap())
            {
                // Load buildings
                // Don't load buildings if it's instance map
                AsyncResponseData<BuildingsResp> buildingsResp;
                do
                {
                    buildingsResp = await DbServiceClient.ReadBuildingsAsync(new ReadBuildingsReq()
                    {
                        MapName = CurrentMapInfo.Id,
                    });
                } while (!buildingsResp.IsSuccess);
                HashSet<StorageId> storageIds = new HashSet<StorageId>();
                List<BuildingSaveData> buildings = buildingsResp.Response.List;
                BuildingEntity buildingEntity;
                foreach (BuildingSaveData building in buildings)
                {
                    buildingEntity = CreateBuildingEntity(building, true);
                    if (buildingEntity is StorageEntity)
                        storageIds.Add(new StorageId(StorageType.Building, (buildingEntity as StorageEntity).Id));
                }
                List<UniTask> tasks = new List<UniTask>();
                // Load building storage
                foreach (StorageId storageId in storageIds)
                {
                    tasks.Add(LoadStorageRoutine(storageId));
                }
                // Wait until all building storage loaded
                await UniTask.WhenAll(tasks);
            }
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        protected override async UniTask PostSpawnEntities()
        {
            await UniTask.Yield();
            ClusterClient.OnAppStart();
        }
#endif

        #region Character spawn function
        public override void SerializeEnterGameData(NetDataWriter writer)
        {
            writer.Put(GameInstance.UserId);
            writer.Put(GameInstance.UserToken);
            writer.Put(GameInstance.SelectedCharacterId);
        }

#if UNITY_EDITOR || UNITY_SERVER
        public override async UniTask<bool> DeserializeEnterGameData(long connectionId, NetDataReader reader)
        {
            string userId = reader.GetString();
            string accessToken = reader.GetString();
            string selectCharacterId = reader.GetString();
            if (!await ValidatePlayerConnection(connectionId, userId, accessToken, selectCharacterId))
            {
                return false;
            }

            CancellationTokenSource cancellationTokenSource;
            BasePlayerCharacterEntity playerCharacterEntity;
            if (despawningPlayerCharacterCancellations.TryGetValue(selectCharacterId, out cancellationTokenSource) &&
                despawningPlayerCharacterEntities.TryGetValue(selectCharacterId, out playerCharacterEntity) &&
                !cancellationTokenSource.IsCancellationRequested)
            {
                // Cancel character despawning to despawning immediately
                despawningPlayerCharacterCancellations.TryRemove(selectCharacterId, out _);
                despawningPlayerCharacterEntities.TryRemove(selectCharacterId, out _);
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                // Save character before despawned
                while (savingCharacters.Contains(selectCharacterId))
                {
                    await UniTask.Yield();
                }
                await SaveCharacter(playerCharacterEntity);
                // Despawn the character
                playerCharacterEntity.NetworkDestroy();
            }
            return true;
        }
#endif

        protected override void HandleEnterGameResponse(ResponseHandlerData responseHandler, AckResponseCode responseCode, EnterGameResponseMessage response)
        {
            base.HandleEnterGameResponse(responseHandler, responseCode, response);
            if (responseCode == AckResponseCode.Success)
            {
                // Disconnect from central server when accepted by map server
                MMOClientInstance.Singleton.StopCentralClient();
            }
        }

        public override void SerializeClientReadyData(NetDataWriter writer)
        {
            writer.Put(GameInstance.UserId);
            writer.Put(GameInstance.UserToken);
            writer.Put(GameInstance.SelectedCharacterId);
        }

#if UNITY_EDITOR || UNITY_SERVER
        public override async UniTask<bool> DeserializeClientReadyData(LiteNetLibIdentity playerIdentity, long connectionId, NetDataReader reader)
        {
            string userId = reader.GetString();
            string accessToken = reader.GetString();
            string selectCharacterId = reader.GetString();
            if (!await ValidatePlayerConnection(connectionId, userId, accessToken, selectCharacterId))
            {
                ServerTransport.ServerDisconnect(connectionId);
                return false;
            }

            if (!IsReadyToInstantiateObjects())
            {
                if (LogWarn)
                    Logging.LogWarning(LogTag, "Not ready to spawn player: " + userId);
                // Add to pending list to spawn player later when map server is ready to instantiate object
                pendingSpawnPlayerCharacters.Add(new PendingSpawnPlayerCharacter()
                {
                    connectionId = connectionId,
                    userId = userId,
                    accessToken = accessToken,
                    selectCharacterId = selectCharacterId,
                });
                return false;
            }

            if (connectionsByUserId.ContainsKey(userId))
                return false;

            accessTokensByConnectionId.TryRemove(connectionId, out _);
            accessTokensByConnectionId.TryAdd(connectionId, accessToken);
            RegisterUserId(connectionId, userId);
            SetPlayerReadyRoutine(connectionId, userId, accessToken, selectCharacterId).Forget();
            return true;
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        private async UniTask<bool> ValidatePlayerConnection(long connectionId, string userId, string accessToken, string selectCharacterId)
        {
            if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out _))
            {
                if (LogError)
                    Logging.LogError(LogTag, "User trying to hack: " + userId);
                return false;
            }

            AsyncResponseData<ValidateAccessTokenResp> validateAccessTokenResp = await DbServiceClient.ValidateAccessTokenAsync(new ValidateAccessTokenReq()
            {
                UserId = userId,
                AccessToken = accessToken,
            });

            if (!validateAccessTokenResp.IsSuccess || !validateAccessTokenResp.Response.IsPass)
            {
                if (LogError)
                    Logging.LogError(LogTag, "Invalid access token for user: " + userId);
                return false;
            }

            return true;
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        private async UniTaskVoid SetPlayerReadyRoutine(long connectionId, string userId, string accessToken, string selectCharacterId)
        {
            AsyncResponseData<CharacterResp> characterResp = await DbServiceClient.ReadCharacterAsync(new ReadCharacterReq()
            {
                UserId = userId,
                CharacterId = selectCharacterId
            });
            if (!characterResp.IsSuccess)
            {
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }
            PlayerCharacterData playerCharacterData = characterResp.Response.CharacterData;
            // If data is empty / cannot find character, disconnect user
            if (playerCharacterData == null)
            {
                if (LogError)
                    Logging.LogError(LogTag, "Cannot find select character: " + selectCharacterId + " for user: " + userId);
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }
            BasePlayerCharacterEntity entityPrefab = playerCharacterData.GetEntityPrefab() as BasePlayerCharacterEntity;
            // If it is not allow this character data, disconnect user
            if (entityPrefab == null)
            {
                if (LogError)
                    Logging.LogError(LogTag, "Cannot find player character with entity Id: " + playerCharacterData.EntityId);
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }
            // Prepare saving location for this character
            string savingCurrentMapName = playerCharacterData.CurrentMapName;
            Vector3 savingCurrentPosition = playerCharacterData.CurrentPosition;

            if (IsInstanceMap())
            {
                playerCharacterData.CurrentPosition = MapInstanceWarpToPosition;
                if (MapInstanceWarpOverrideRotation)
                    playerCharacterData.CurrentRotation = MapInstanceWarpToRotation;
            }

            // Spawn character entity and set its data
            Quaternion characterRotation = Quaternion.identity;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                characterRotation = Quaternion.Euler(playerCharacterData.CurrentRotation);
            LiteNetLibIdentity spawnObj = Assets.GetObjectInstance(
                entityPrefab.Identity.HashAssetId,
                playerCharacterData.CurrentPosition,
                characterRotation);
            BasePlayerCharacterEntity playerCharacterEntity = spawnObj.GetComponent<BasePlayerCharacterEntity>();
            playerCharacterData.CloneTo(playerCharacterEntity);

            // Set currencies
            // Gold
            AsyncResponseData<GoldResp> getGoldResp = await DbServiceClient.GetGoldAsync(new GetGoldReq()
            {
                UserId = userId,
            });
            if (!getGoldResp.IsSuccess)
            {
                Destroy(spawnObj.gameObject);
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }
            playerCharacterEntity.UserGold = getGoldResp.Response.Gold;
            // Cash
            AsyncResponseData<CashResp> getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
            {
                UserId = userId,
            });
            if (!getCashResp.IsSuccess)
            {
                Destroy(spawnObj.gameObject);
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }
            playerCharacterEntity.UserCash = getCashResp.Response.Cash;

            // Set user Id
            playerCharacterEntity.UserId = userId;

            // Load user level
            AsyncResponseData<GetUserLevelResp> getUserLevelResp = await DbServiceClient.GetUserLevelAsync(new GetUserLevelReq()
            {
                UserId = userId,
                AccessToken = accessToken,
            });
            if (!getUserLevelResp.IsSuccess)
            {
                Destroy(spawnObj.gameObject);
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }
            playerCharacterEntity.UserLevel = getUserLevelResp.Response.UserLevel;

            // Load party data, if this map-server does not have party data
            if (playerCharacterEntity.PartyId > 0)
            {
                if (!ServerPartyHandlers.ContainsParty(playerCharacterEntity.PartyId))
                    await LoadPartyRoutine(playerCharacterEntity.PartyId);
                if (ServerPartyHandlers.TryGetParty(playerCharacterEntity.PartyId, out PartyData party))
                {
                    ServerGameMessageHandlers.SendSetFullPartyData(connectionId, party);
                }
                else
                {
                    playerCharacterEntity.ClearParty();
                }
            }

            // Load guild data, if this map-server does not have guild data
            if (playerCharacterEntity.GuildId > 0)
            {
                if (!ServerGuildHandlers.ContainsGuild(playerCharacterEntity.GuildId))
                    await LoadGuildRoutine(playerCharacterEntity.GuildId);
                if (ServerGuildHandlers.TryGetGuild(playerCharacterEntity.GuildId, out GuildData guild))
                {
                    playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                    ServerGameMessageHandlers.SendSetFullGuildData(connectionId, guild);
                }
                else
                {
                    playerCharacterEntity.ClearGuild();
                }
            }

            // Force make caches, to calculate current stats to fill empty slots items
            playerCharacterEntity.ForceMakeCaches();
            playerCharacterEntity.FillEmptySlots();

            if (!playerCharacterEntity.IsDead())
            {
                // Summon saved summons
                AsyncResponseData<GetSummonBuffsResp> summonBuffsResp = await DbServiceClient.GetSummonBuffsAsync(new GetSummonBuffsReq()
                {
                    CharacterId = playerCharacterEntity.Id,
                });
                if (!summonBuffsResp.IsSuccess)
                {
                    Destroy(spawnObj.gameObject);
                    ServerTransport.ServerDisconnect(connectionId);
                    return;
                }

                List<CharacterBuff> summonBuffs = summonBuffsResp.Response.SummonBuffs;
                for (int i = 0; i < playerCharacterEntity.Summons.Count; ++i)
                {
                    CharacterSummon summon = playerCharacterEntity.Summons[i];
                    summon.Summon(playerCharacterEntity, summon.Level, summon.summonRemainsDuration, summon.Exp, summon.CurrentHp, summon.CurrentMp);
                    for (int j = 0; j < summonBuffs.Count; ++j)
                    {
                        if (summonBuffs[j].id.StartsWith(i.ToString()))
                        {
                            summon.CacheEntity.Buffs.Add(summonBuffs[j]);
                            summonBuffs.RemoveAt(j);
                            j--;
                        }
                    }
                    playerCharacterEntity.Summons[i] = summon;
                }
            }

            // Make sure that player does not exit before character data loaded
            if (!ContainsConnectionId(connectionId))
            {
                Destroy(spawnObj.gameObject);
                ServerTransport.ServerDisconnect(connectionId);
                return;
            }

            // Spawn the character
            Assets.NetworkSpawn(spawnObj, 0, connectionId);

            // Register player character entity to the server
            RegisterPlayerCharacter(connectionId, playerCharacterEntity);

            // Don't destroy player character entity when disconnect
            playerCharacterEntity.Identity.DoNotDestroyWhenDisconnect = true;

            // Notify clients that this character is spawn or dead
            if (!playerCharacterEntity.IsDead())
            {
                playerCharacterEntity.CallAllOnRespawn();
                // Summon saved mount entity
                if (GameInstance.VehicleEntities.ContainsKey(playerCharacterData.MountDataId))
                    playerCharacterEntity.Mount(GameInstance.VehicleEntities[playerCharacterData.MountDataId]);
            }
            else
            {
                playerCharacterEntity.CallAllOnDead();
            }

            // Prepare saving location for this character
            if (IsInstanceMap())
            {
                instanceMapCurrentLocations.TryAdd(playerCharacterEntity.ObjectId, new KeyValuePair<string, Vector3>(savingCurrentMapName, savingCurrentPosition));
            }
        }
#endif
        #endregion

        #region Network message handlers
        protected override void HandleWarpAtClient(MessageHandlerData messageHandler)
        {
            MMOWarpMessage message = messageHandler.ReadMessage<MMOWarpMessage>();
            Assets.offlineScene.SceneName = string.Empty;
            StopClient();
            StartClient(message.networkAddress, message.networkPort);
        }

#if UNITY_EDITOR || UNITY_SERVER
        protected override void HandleChatAtServer(MessageHandlerData messageHandler)
        {
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>().FillChannelId();
            string userId = string.Empty;
            string accessToken = string.Empty;
            IPlayerCharacterData playerCharacter;
            if (messageHandler.ConnectionId >= 0 && message.sendByServer)
            {
                // This message should be sent by server but its connection >= 0, which means it is not a server ;)
                return;
            }
            else
            {
                // Get character
                if (!ServerUserHandlers.TryGetPlayerCharacter(messageHandler.ConnectionId, out playerCharacter))
                {
                    // Not allow to enter chat
                    return;
                }
                message.senderId = playerCharacter.Id;
                message.senderUserId = playerCharacter.UserId;
                message.senderName = playerCharacter.CharacterName;
                userId = playerCharacter.UserId;
                if (!accessTokensByConnectionId.TryGetValue(messageHandler.ConnectionId, out accessToken))
                    accessToken = string.Empty;
            }
            // Set guild data
            if (playerCharacter != null)
            {
                if (ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guildData))
                {
                    message.guildId = playerCharacter.GuildId;
                    message.guildName = guildData.guildName;
                }
            }
            // Character muted?
            if (!message.sendByServer && playerCharacter != null && playerCharacter.IsMuting())
            {
                ServerSendPacket(messageHandler.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, new ChatMessage()
                {
                    channel = ChatChannel.System,
                    message = "You have been muted.",
                });
                return;
            }
            // Local chat will processes immediately, not have to be sent to cluster server
            if (message.channel == ChatChannel.Local)
            {
                bool sentGmCommand = false;
                if (message.sendByServer || playerCharacter != null)
                {
                    if (message.sendByServer || (playerCharacter is BasePlayerCharacterEntity playerCharacterEntity &&
                        CurrentGameInstance.GMCommands.IsGMCommand(message.message, out string gmCommand) &&
                        CurrentGameInstance.GMCommands.CanUseGMCommand(playerCharacterEntity.UserLevel, gmCommand)))
                    {
                        // If it's GM command and senderName's user level > 0, handle gm commands
                        // Send GM command to cluster server to broadcast to other servers later
                        if (ClusterClient.IsNetworkActive)
                        {
                            ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                            {
                                writer.PutValue(message);
                                writer.Put(userId);
                                writer.Put(accessToken);
                            });
                        }
                        sentGmCommand = true;
                    }
                }
                if (!sentGmCommand)
                    ServerChatHandlers.OnChatMessage(message);
                return;
            }
            if (message.channel == ChatChannel.System)
            {
                if (ServerChatHandlers.CanSendSystemAnnounce(message.senderName))
                {
                    // Send chat message to cluster server, for MMO mode chat message handling by cluster server
                    if (ClusterClient.IsNetworkActive)
                    {
                        ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                        {
                            writer.PutValue(message);
                            writer.Put(userId);
                            writer.Put(accessToken);
                        });
                    }
                }
                return;
            }
            // Send chat message to chat server, for MMO mode chat message handling by chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                {
                    writer.PutValue(message);
                    writer.Put(userId);
                    writer.Put(accessToken);
                });
            }
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        private void OnResponseAppServerRegister(AckResponseCode responseCode)
        {
            if (responseCode != AckResponseCode.Success)
                return;
            UpdateMapUsers(ClusterClient, UpdateUserCharacterMessage.UpdateType.Add);
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        private void OnResponseAppServerAddress(AckResponseCode responseCode, CentralServerPeerInfo peerInfo)
        {
            if (responseCode != AckResponseCode.Success)
                return;
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
                        HashSet<uint> warpingCharacters;
                        if (instanceMapWarpingCharactersByInstanceId.TryGetValue(peerInfo.extra, out warpingCharacters))
                        {
                            BasePlayerCharacterEntity warpingCharacterEntity;
                            foreach (uint warpingCharacter in warpingCharacters)
                            {
                                if (!Assets.TryGetSpawnedObject(warpingCharacter, out warpingCharacterEntity))
                                    continue;
                                WarpCharacterToInstanceRoutine(warpingCharacterEntity, peerInfo.extra).Forget();
                            }
                        }
                    }
                    break;
            }
        }
#endif
        #endregion

        #region Social message handlers
        internal async void HandleChat(MessageHandlerData messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            if (message.channel == ChatChannel.Local)
            {
                // Handle GM command here, only GM command will be broadcasted as local channel
                if (!CurrentGameInstance.GMCommands.IsGMCommand(message.message, out string gmCommand))
                {
                    // Handle only GM command, if it is not GM command then skip
                    return;
                }

                // Get user level from server to validate user level
                AsyncResponseData<GetUserLevelResp> resp = await DbServiceClient.GetUserLevelAsync(new GetUserLevelReq()
                {
                    UserId = messageHandler.Reader.GetString(),
                    AccessToken = messageHandler.Reader.GetString(),
                });
                if (!resp.IsSuccess)
                {
                    // Error occuring when retreiving user level
                    return;
                }
                if (!CurrentGameInstance.GMCommands.CanUseGMCommand(resp.Response.UserLevel, gmCommand))
                {
                    // User not able to use this command
                    return;
                }

                // Response enter GM command message
                if (!ServerUserHandlers.TryGetPlayerCharacterByName(message.senderName, out BasePlayerCharacterEntity playerCharacterEntity))
                    playerCharacterEntity = null;
                string response = CurrentGameInstance.GMCommands.HandleGMCommand(message.senderName, playerCharacterEntity, message.message);
                if (playerCharacterEntity != null && !string.IsNullOrEmpty(response))
                {
                    ServerSendPacket(playerCharacterEntity.ConnectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, new ChatMessage()
                    {
                        channel = ChatChannel.System,
                        message = response,
                    });
                }
            }
            else
            {
                ServerChatHandlers.OnChatMessage(message);
            }
#endif
        }

        internal void HandleUpdateMapUser(MessageHandlerData messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            int socialId;
            PartyData party;
            GuildData guild;
            switch (message.type)
            {
                case UpdateUserCharacterMessage.UpdateType.Add:
                    if (!usersById.ContainsKey(message.character.id))
                        usersById.TryAdd(message.character.id, message.character);
                    break;
                case UpdateUserCharacterMessage.UpdateType.Remove:
                    usersById.TryRemove(message.character.id, out _);
                    break;
                case UpdateUserCharacterMessage.UpdateType.Online:
                    if (usersById.ContainsKey(message.character.id))
                    {
                        ServerCharacterHandlers.MarkOnlineCharacter(message.character.id);
                        socialId = message.character.partyId;
                        if (socialId > 0 && ServerPartyHandlers.TryGetParty(socialId, out party))
                        {
                            party.UpdateMember(message.character);
                            ServerPartyHandlers.SetParty(socialId, party);
                        }
                        socialId = message.character.guildId;
                        if (socialId > 0 && ServerGuildHandlers.TryGetGuild(socialId, out guild))
                        {
                            guild.UpdateMember(message.character);
                            ServerGuildHandlers.SetGuild(socialId, guild);
                        }
                        usersById[message.character.id] = message.character;
                    }
                    break;
            }
#endif
        }

        internal void HandleUpdatePartyMember(MessageHandlerData messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            PartyData party;
            BasePlayerCharacterEntity playerCharacterEntity;
            if (ServerPartyHandlers.TryGetParty(message.socialId, out party) && party.UpdateSocialGroupMember(message))
            {
                switch (message.type)
                {
                    case UpdateSocialMemberMessage.UpdateType.Add:
                        if (ServerUserHandlers.TryGetPlayerCharacterById(message.character.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.PartyId = message.socialId;
                            ServerGameMessageHandlers.SendSetPartyData(playerCharacterEntity.ConnectionId, party);
                            ServerGameMessageHandlers.SendAddPartyMembersToOne(playerCharacterEntity.ConnectionId, party);
                        }
                        ServerGameMessageHandlers.SendAddPartyMemberToMembers(party, message.character);
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (ServerUserHandlers.TryGetPlayerCharacterById(message.character.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.ClearParty();
                            ServerGameMessageHandlers.SendClearPartyData(playerCharacterEntity.ConnectionId, message.socialId);
                        }
                        ServerGameMessageHandlers.SendRemovePartyMemberToMembers(party, message.character.id);
                        break;
                }
            }
#endif
        }

        internal void HandleUpdateParty(MessageHandlerData messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            BasePlayerCharacterEntity playerCharacterEntity;
            PartyData party;
            if (ServerPartyHandlers.TryGetParty(message.id, out party))
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        party.SetLeader(message.characterId);
                        ServerPartyHandlers.SetParty(message.id, party);
                        ServerGameMessageHandlers.SendSetPartyLeaderToMembers(party);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        party.Setting(message.shareExp, message.shareItem);
                        ServerPartyHandlers.SetParty(message.id, party);
                        ServerGameMessageHandlers.SendSetPartySettingToMembers(party);
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        foreach (string memberId in party.GetMemberIds())
                        {
                            if (ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacterEntity))
                            {
                                playerCharacterEntity.ClearParty();
                                ServerGameMessageHandlers.SendClearPartyData(playerCharacterEntity.ConnectionId, message.id);
                            }
                        }
                        ServerPartyHandlers.RemoveParty(message.id);
                        break;
                }
            }
#endif
        }

        internal void HandleUpdateGuildMember(MessageHandlerData messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            GuildData guild;
            BasePlayerCharacterEntity playerCharacterEntity;
            if (ServerGuildHandlers.TryGetGuild(message.socialId, out guild) && guild.UpdateSocialGroupMember(message))
            {
                switch (message.type)
                {
                    case UpdateSocialMemberMessage.UpdateType.Add:
                        if (ServerUserHandlers.TryGetPlayerCharacterById(message.character.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.GuildId = message.socialId;
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                            ServerGameMessageHandlers.SendSetGuildData(playerCharacterEntity.ConnectionId, guild);
                            ServerGameMessageHandlers.SendAddGuildMembersToOne(playerCharacterEntity.ConnectionId, guild);
                        }
                        ServerGameMessageHandlers.SendAddGuildMemberToMembers(guild, message.character);
                        break;
                    case UpdateSocialMemberMessage.UpdateType.Remove:
                        if (ServerUserHandlers.TryGetPlayerCharacterById(message.character.id, out playerCharacterEntity))
                        {
                            playerCharacterEntity.ClearGuild();
                            ServerGameMessageHandlers.SendClearGuildData(playerCharacterEntity.ConnectionId, message.socialId);
                        }
                        ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(guild, message.character.id);
                        break;
                }
            }
#endif
        }

        internal void HandleUpdateGuild(MessageHandlerData messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            BasePlayerCharacterEntity playerCharacterEntity;
            GuildData guild;
            if (ServerGuildHandlers.TryGetGuild(message.id, out guild))
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        guild.SetLeader(message.characterId);
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        if (ServerUserHandlers.TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                        ServerGameMessageHandlers.SendSetGuildLeaderToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        guild.guildMessage = message.guildMessage;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildMessageToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage2:
                        guild.guildMessage2 = message.guildMessage;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildMessageToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        guild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacterEntity))
                                playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                        }
                        ServerGameMessageHandlers.SendSetGuildRoleToMembers(guild, message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        guild.SetMemberRole(message.characterId, message.guildRole);
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        if (ServerUserHandlers.TryGetPlayerCharacterById(message.characterId, out playerCharacterEntity))
                            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                        ServerGameMessageHandlers.SendSetGuildMemberRoleToMembers(guild, message.characterId, message.guildRole);
                        break;
                    case UpdateGuildMessage.UpdateType.SetSkillLevel:
                        guild.SetSkillLevel(message.dataId, message.level);
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildSkillLevelToMembers(guild, message.dataId);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGold:
                        guild.gold = message.gold;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetScore:
                        guild.score = message.score;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildScoreToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetOptions:
                        guild.options = message.options;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildOptionsToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetAutoAcceptRequests:
                        guild.autoAcceptRequests = message.autoAcceptRequests;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildAutoAcceptRequestsToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.SetRank:
                        guild.rank = message.rank;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildRankToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.LevelExpSkillPoint:
                        guild.level = message.level;
                        guild.exp = message.exp;
                        guild.skillPoint = message.skillPoint;
                        ServerGuildHandlers.SetGuild(message.id, guild);
                        ServerGameMessageHandlers.SendSetGuildLevelExpSkillPointToMembers(guild);
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacterEntity))
                            {
                                playerCharacterEntity.ClearGuild();
                                ServerGameMessageHandlers.SendClearGuildData(playerCharacterEntity.ConnectionId, message.id);
                            }
                        }
                        ServerGuildHandlers.RemoveGuild(message.id);
                        break;
                }
            }
#endif
        }
        #endregion

        #region Update map user functions
        private void UpdateMapUsers(LiteNetLibClient transportHandler, UpdateUserCharacterMessage.UpdateType updateType)
        {
#if UNITY_EDITOR || UNITY_SERVER
            foreach (SocialCharacterData user in usersById.Values)
            {
                UpdateMapUser(transportHandler, updateType, user);
            }
#endif
        }

        private void UpdateMapUser(LiteNetLibClient transportHandler, UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData)
        {
#if UNITY_EDITOR || UNITY_SERVER
            UpdateUserCharacterMessage updateMapUserMessage = new UpdateUserCharacterMessage();
            updateMapUserMessage.type = updateType;
            updateMapUserMessage.character = userData;
            transportHandler.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, updateMapUserMessage.Serialize);
#endif
        }
        #endregion

        public void KickUserById(string userId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            long connectionId;
            if (connectionsByUserId.TryGetValue(userId, out connectionId))
                ServerTransport.ServerDisconnect(connectionId);
#endif
        }
    }
}
