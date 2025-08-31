using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(LogGUI))]
    [DefaultExecutionOrder(DefaultExecutionOrders.MMO_SERVER_INSTANCE)]
    public class MMOServerInstance : MonoBehaviour
    {
        public static MMOServerInstance Singleton { get; protected set; }

        [Header("Server Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager = null;
        [SerializeField]
        private MapSpawnNetworkManager mapSpawnNetworkManager = null;
        [SerializeField]
        private MapNetworkManager mapNetworkManager = null;
        [SerializeField]
        private DatabaseNetworkManager databaseNetworkManager = null;
        [SerializeField]
        [Tooltip("Use custom database client or not, if yes, it won't use `databaseNetworkManager` for network management")]
        private bool useCustomDatabaseClient = false;
        [SerializeField]
        [Tooltip("Which game object has a custom database client attached")]
        private GameObject customDatabaseClientSource = null;

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;
        [SerializeField]
        private bool webSocketSecure = false;
        [SerializeField]
        private string webSocketCertPath = string.Empty;
        [SerializeField]
        private string webSocketCertPassword = string.Empty;

        public CentralNetworkManager CentralNetworkManager
        {
            get
            {
                if (centralNetworkManager == null)
                    centralNetworkManager = GetComponentInChildren<CentralNetworkManager>();
                if (centralNetworkManager == null)
                    centralNetworkManager = FindFirstObjectByType<CentralNetworkManager>();
                return centralNetworkManager;
            }
        }
        public MapSpawnNetworkManager MapSpawnNetworkManager
        {
            get
            {
                if (mapSpawnNetworkManager == null)
                    mapSpawnNetworkManager = GetComponentInChildren<MapSpawnNetworkManager>();
                if (mapSpawnNetworkManager == null)
                    mapSpawnNetworkManager = FindFirstObjectByType<MapSpawnNetworkManager>();
                return mapSpawnNetworkManager;
            }
        }
        public MapNetworkManager MapNetworkManager
        {
            get
            {
                if (mapNetworkManager == null)
                    mapNetworkManager = GetComponentInChildren<MapNetworkManager>();
                if (mapNetworkManager == null)
                    mapNetworkManager = FindFirstObjectByType<MapNetworkManager>();
                return mapNetworkManager;
            }
        }
        public IDatabaseClient DatabaseClient
        {
            get
            {
                if (!useCustomDatabaseClient)
                    return databaseNetworkManager;
                else
                    return _customDatabaseClient;
            }
        }
        public IChatProfanityDetector ChatProfanityDetector { get; private set; }
        public bool UseWebSocket
        {
            get => useWebSocket;
            set => useWebSocket = value;
        }
        public bool WebSocketSecure
        {
            get => webSocketSecure;
            set => webSocketSecure = value;
        }
        public string WebSocketCertificateFilePath
        {
            get => webSocketCertPath;
            set => webSocketCertPath = value;
        }
        public string WebSocketCertificatePassword
        {
            get => webSocketCertPassword;
            set => webSocketCertPassword = value;
        }

        private LogGUI _cacheLogGUI;
        public LogGUI CacheLogGUI
        {
            get
            {
                if (_cacheLogGUI == null)
                    _cacheLogGUI = GetComponent<LogGUI>();
                return _cacheLogGUI;
            }
        }

        public string LogFileName { get; set; }

        [Header("Running In Editor")]
        public bool startCentralOnAwake;
        public bool startMapSpawnOnAwake;
        public bool startDatabaseOnAwake;
        public bool startMapOnAwake;
        public BaseMapInfo startingMap;
        public int databaseOptionIndex;
        [FormerlySerializedAs("databaseDisableCacheReading")]
        public bool disableDatabaseCaching;

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private List<string> _spawningMaps;
        private List<SpawnAllocateMapByNameData> _spawningAllocateMaps;
        private string _startingMapId;
        private bool _startingCentralServer;
        private bool _startingMapSpawnServer;
        private bool _startingMapServer;
        private bool _startingDatabaseServer;
#endif
        private IDatabaseClient _customDatabaseClient;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            Application.wantsToQuit += Application_wantsToQuit;
            GameInstance.OnGameDataLoadedEvent += OnGameDataLoaded;
#endif

            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

            // Setup custom database client
            if (customDatabaseClientSource == null)
                customDatabaseClientSource = gameObject;
            _customDatabaseClient = customDatabaseClientSource.GetComponent<IDatabaseClient>();
            ChatProfanityDetector = GetComponentInChildren<IChatProfanityDetector>();
            if (ChatProfanityDetector == null)
                ChatProfanityDetector = gameObject.AddComponent<DisabledChatProfanityDetector>();

            CacheLogGUI.enabled = false;
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!Application.isEditor)
            {
                // Prepare data
                string[] args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

                bool configFileFound = ConfigManager.HasServerConfig();
                ServerConfig currentServerConfig = ConfigManager.ReadServerConfig();

                // Use custom database client or not?
                bool useCustomDatabaseClient = this.useCustomDatabaseClient = false;
                if (_customDatabaseClient != null && _customDatabaseClient as UnityEngine.Object != null)
                {
                    if (ConfigReader.IsArgsProvided(args, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT))
                    {
                        useCustomDatabaseClient = true;
                    }
                    else if (currentServerConfig.useCustomDatabaseClient.HasValue)
                    {
                        useCustomDatabaseClient = currentServerConfig.useCustomDatabaseClient.Value;
                    }
                }
                currentServerConfig.useCustomDatabaseClient = this.useCustomDatabaseClient = useCustomDatabaseClient;

                // Database option index
                int databaseOptionIndex = 0;
                if (!useCustomDatabaseClient)
                {
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_OPTION_INDEX, out databaseOptionIndex, 0))
                    {
                        databaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);
                    }
                    else if (currentServerConfig.databaseOptionIndex.HasValue)
                    {
                        databaseOptionIndex = currentServerConfig.databaseOptionIndex.Value;
                        databaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);
                    }
                }
                currentServerConfig.databaseOptionIndex = databaseOptionIndex;

                // Database disable cache reading or not?
                bool disableDatabaseCaching = this.disableDatabaseCaching = false;
                // Old config key
#pragma warning disable CS0618 // Type or member is obsolete
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DATABASE_DISABLE_CACHE_READING))
                {
                    disableDatabaseCaching = true;
                }
                if (currentServerConfig.databaseDisableCacheReading.HasValue && !currentServerConfig.disableDatabaseCaching.HasValue)
                {
                    currentServerConfig.disableDatabaseCaching = currentServerConfig.databaseDisableCacheReading;
                    currentServerConfig.databaseDisableCacheReading = null;
                }
#pragma warning restore CS0618 // Type or member is obsolete
                // New config key
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DISABLE_DATABASE_CACHING))
                {
                    disableDatabaseCaching = true;
                }
                else if (currentServerConfig.disableDatabaseCaching.HasValue)
                {
                    disableDatabaseCaching = currentServerConfig.disableDatabaseCaching.Value;
                }
                currentServerConfig.disableDatabaseCaching = this.disableDatabaseCaching = disableDatabaseCaching;

                // Use Websocket or not?
                bool useWebSocket = this.useWebSocket = false;
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_USE_WEB_SOCKET))
                {
                    useWebSocket = true;
                }
                else if (currentServerConfig.useWebSocket.HasValue)
                {
                    useWebSocket = currentServerConfig.useWebSocket.Value;
                }
                currentServerConfig.useWebSocket = this.useWebSocket = useWebSocket;

                // Is websocket running in secure mode or not?
                bool webSocketSecure = this.webSocketSecure = false; 
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_WEB_SOCKET_SECURE))
                {
                    webSocketSecure = true;
                }
                else if (currentServerConfig.webSocketSecure.HasValue)
                {
                    webSocketSecure = currentServerConfig.webSocketSecure.Value;
                }
                currentServerConfig.webSocketSecure = this.webSocketSecure = webSocketSecure;

                // Where is the certification file path?
                string webSocketCertPath;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath))
                {
                    this.webSocketCertPath = webSocketCertPath;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.webSocketCertPath))
                {
                    this.webSocketCertPath = webSocketCertPath = currentServerConfig.webSocketCertPath;
                }
                currentServerConfig.webSocketCertPath = webSocketCertPath;

                // What is the certification password?
                string webSocketCertPassword;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword))
                {
                    this.webSocketCertPassword = webSocketCertPassword;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.webSocketCertPassword))
                {
                    this.webSocketCertPassword = webSocketCertPassword = currentServerConfig.webSocketCertPassword;
                }
                currentServerConfig.webSocketCertPassword = webSocketCertPassword;

                // Central network address
                string centralAddress;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_ADDRESS, out centralAddress, MapNetworkManager.clusterServerAddress))
                {
                    MapNetworkManager.clusterServerAddress = centralAddress;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.centralAddress))
                {
                    MapNetworkManager.clusterServerAddress = centralAddress = currentServerConfig.centralAddress;
                }
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_ADDRESS, out centralAddress, MapSpawnNetworkManager.clusterServerAddress))
                {
                    MapSpawnNetworkManager.clusterServerAddress = centralAddress;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.centralAddress))
                {
                    MapSpawnNetworkManager.clusterServerAddress = centralAddress = currentServerConfig.centralAddress;
                }
                currentServerConfig.centralAddress = centralAddress;

                // Central network port
                int centralPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_PORT, out centralPort, CentralNetworkManager.networkPort))
                {
                    CentralNetworkManager.networkPort = centralPort;
                }
                else if (currentServerConfig.centralPort.HasValue)
                {
                    CentralNetworkManager.networkPort = centralPort = currentServerConfig.centralPort.Value;
                }
                currentServerConfig.centralPort = centralPort;

                // Central max connections
                int centralMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, CentralNetworkManager.maxConnections))
                {
                    CentralNetworkManager.maxConnections = centralMaxConnections;
                }
                else if (currentServerConfig.centralMaxConnections.HasValue)
                {
                    CentralNetworkManager.maxConnections = centralMaxConnections = currentServerConfig.centralMaxConnections.Value;
                }
                currentServerConfig.centralMaxConnections = centralMaxConnections;

                // Central map spawn timeout (milliseconds)
                int mapSpawnMillisecondsTimeout;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_MILLISECONDS_TIMEOUT, out mapSpawnMillisecondsTimeout, CentralNetworkManager.mapSpawnMillisecondsTimeout))
                {
                    CentralNetworkManager.mapSpawnMillisecondsTimeout = mapSpawnMillisecondsTimeout;
                }
                else if (currentServerConfig.mapSpawnMillisecondsTimeout.HasValue)
                {
                    CentralNetworkManager.mapSpawnMillisecondsTimeout = mapSpawnMillisecondsTimeout = currentServerConfig.mapSpawnMillisecondsTimeout.Value;
                }
                currentServerConfig.mapSpawnMillisecondsTimeout = mapSpawnMillisecondsTimeout;

                // Central - default channels max connections
                int defaultChannelMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DEFAULT_CHANNEL_MAX_CONNECTIONS, out defaultChannelMaxConnections, CentralNetworkManager.defaultChannelMaxConnections))
                {
                    CentralNetworkManager.defaultChannelMaxConnections = defaultChannelMaxConnections;
                }
                else if (currentServerConfig.defaultChannelMaxConnections.HasValue)
                {
                    CentralNetworkManager.defaultChannelMaxConnections = defaultChannelMaxConnections = currentServerConfig.defaultChannelMaxConnections.Value;
                }
                currentServerConfig.defaultChannelMaxConnections = defaultChannelMaxConnections;

                // Central - channels
                if (currentServerConfig.channels == null)
                    currentServerConfig.channels = CentralNetworkManager.channels;
                CentralNetworkManager.channels = currentServerConfig.channels;

                // Central->Cluster network port
                int clusterPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLUSTER_PORT, out clusterPort, MapNetworkManager.clusterServerPort))
                {
                    MapNetworkManager.clusterServerPort = clusterPort;
                }
                else if (currentServerConfig.clusterPort.HasValue)
                {
                    MapNetworkManager.clusterServerPort = clusterPort = currentServerConfig.clusterPort.Value;
                }
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLUSTER_PORT, out clusterPort, MapSpawnNetworkManager.clusterServerPort))
                {
                    MapSpawnNetworkManager.clusterServerPort = clusterPort;
                }
                else if (currentServerConfig.clusterPort.HasValue)
                {
                    MapSpawnNetworkManager.clusterServerPort = clusterPort = currentServerConfig.clusterPort.Value;
                }
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLUSTER_PORT, out clusterPort, CentralNetworkManager.clusterServerPort))
                {
                    CentralNetworkManager.clusterServerPort = clusterPort;
                }
                else if (currentServerConfig.clusterPort.HasValue)
                {
                    CentralNetworkManager.clusterServerPort = clusterPort = currentServerConfig.clusterPort.Value;
                }
                currentServerConfig.clusterPort = clusterPort;

                // Machine network address, will be set to map spawn / map / chat
                string publicAddress;
                // Old config key
#pragma warning disable CS0618 // Type or member is obsolete
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MACHINE_ADDRESS, out publicAddress, MapNetworkManager.publicAddress))
                {
                    MapNetworkManager.publicAddress = publicAddress;
                }
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MACHINE_ADDRESS, out publicAddress, MapSpawnNetworkManager.publicAddress))
                {
                    MapSpawnNetworkManager.publicAddress = publicAddress;
                }
                if (!string.IsNullOrEmpty(currentServerConfig.machineAddress) && string.IsNullOrEmpty(currentServerConfig.publicAddress))
                {
                    currentServerConfig.publicAddress = currentServerConfig.machineAddress;
                    currentServerConfig.machineAddress = null;
                }
#pragma warning restore CS0618 // Type or member is obsolete
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_PUBLIC_ADDRESS, out publicAddress, MapNetworkManager.publicAddress))
                {
                    MapNetworkManager.publicAddress = publicAddress;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.publicAddress))
                {
                    MapNetworkManager.publicAddress = publicAddress = currentServerConfig.publicAddress;
                }
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_PUBLIC_ADDRESS, out publicAddress, MapSpawnNetworkManager.publicAddress))
                {
                    MapSpawnNetworkManager.publicAddress = publicAddress;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.publicAddress))
                {
                    MapSpawnNetworkManager.publicAddress = publicAddress = currentServerConfig.publicAddress;
                }
                currentServerConfig.publicAddress = publicAddress;

                // Map spawn network port
                int mapSpawnPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_PORT, out mapSpawnPort, MapSpawnNetworkManager.networkPort))
                {
                    MapSpawnNetworkManager.networkPort = mapSpawnPort;
                }
                else if (currentServerConfig.mapSpawnPort.HasValue)
                {
                    MapSpawnNetworkManager.networkPort = mapSpawnPort = currentServerConfig.mapSpawnPort.Value;
                }
                currentServerConfig.mapSpawnPort = mapSpawnPort;

                // Map spawn exe path
                string spawnExePath;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_EXE_PATH, out spawnExePath, MapSpawnNetworkManager.spawnExePath))
                {
                    MapSpawnNetworkManager.spawnExePath = spawnExePath;
                }
                else if (!string.IsNullOrEmpty(currentServerConfig.spawnExePath))
                {
                    MapSpawnNetworkManager.spawnExePath = spawnExePath = currentServerConfig.spawnExePath;
                }
                currentServerConfig.spawnExePath = spawnExePath;

                // Map spawn in batch mode
                bool notSpawnInBatchMode = MapSpawnNetworkManager.notSpawnInBatchMode = false;
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_NOT_SPAWN_IN_BATCH_MODE))
                {
                    MapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode = true;
                }
                else if (currentServerConfig.notSpawnInBatchMode.HasValue)
                {
                    MapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode = currentServerConfig.notSpawnInBatchMode.Value;
                }
                currentServerConfig.notSpawnInBatchMode = notSpawnInBatchMode;

                // Map spawn start port
                int spawnStartPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_START_PORT, out spawnStartPort, MapSpawnNetworkManager.startPort))
                {
                    MapSpawnNetworkManager.startPort = spawnStartPort;
                }
                else if (currentServerConfig.spawnStartPort.HasValue)
                {
                    MapSpawnNetworkManager.startPort = spawnStartPort = currentServerConfig.spawnStartPort.Value;
                }
                currentServerConfig.spawnStartPort = spawnStartPort;

                // Spawn channels
                List<string> spawnChannels;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_CHANNELS, out spawnChannels, MapSpawnNetworkManager.spawningChannelIds))
                {
                    MapSpawnNetworkManager.spawningChannelIds = spawnChannels;
                }
                else if (currentServerConfig.spawnChannels != null)
                {
                    MapSpawnNetworkManager.spawningChannelIds = spawnChannels = currentServerConfig.spawnChannels;
                }
                currentServerConfig.spawnChannels = spawnChannels;

                // Spawn maps
                List<string> defaultSpawnMaps = new List<string>();
                foreach (BaseMapInfo mapInfo in MapSpawnNetworkManager.spawningMaps)
                {
                    if (mapInfo != null)
                        defaultSpawnMaps.Add(mapInfo.Id);
                }
                List<string> spawnMaps;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_MAPS, out spawnMaps, defaultSpawnMaps))
                {
                    _spawningMaps = spawnMaps;
                }
                else if (currentServerConfig.spawnMaps != null)
                {
                    _spawningMaps = spawnMaps = currentServerConfig.spawnMaps;
                }
                currentServerConfig.spawnMaps = spawnMaps;

                // Spawn allocate maps
                List<SpawnAllocateMapByNameData> defaultSpawnAllocateMaps = new List<SpawnAllocateMapByNameData>();
                foreach (SpawnAllocateMapData spawnAllocateMap in MapSpawnNetworkManager.spawningAllocateMaps)
                {
                    if (spawnAllocateMap.mapInfo != null)
                    {
                        defaultSpawnAllocateMaps.Add(new SpawnAllocateMapByNameData()
                        {
                            mapName = spawnAllocateMap.mapInfo.Id,
                            allocateAmount = spawnAllocateMap.allocateAmount,
                        });
                    }
                }
                List<SpawnAllocateMapByNameData> spawnAllocateMaps = defaultSpawnAllocateMaps;
                if (currentServerConfig.spawnAllocateMaps != null)
                {
                    _spawningAllocateMaps = spawnAllocateMaps = currentServerConfig.spawnAllocateMaps;
                }
                currentServerConfig.spawnAllocateMaps = spawnAllocateMaps;

                // Map network port
                int mapPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_PORT, out mapPort, MapNetworkManager.networkPort))
                {
                    MapNetworkManager.networkPort = mapPort;
                }
                else if (currentServerConfig.mapPort.HasValue)
                {
                    MapNetworkManager.networkPort = mapPort = currentServerConfig.mapPort.Value;
                }
                currentServerConfig.mapPort = mapPort;

                // Map max connections
                int mapMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_MAX_CONNECTIONS, out mapMaxConnections, MapNetworkManager.maxConnections))
                {
                    MapNetworkManager.maxConnections = mapMaxConnections;
                }
                else if (currentServerConfig.mapMaxConnections.HasValue)
                {
                    MapNetworkManager.maxConnections = mapMaxConnections = currentServerConfig.mapMaxConnections.Value;
                }
                currentServerConfig.mapMaxConnections = mapMaxConnections;

                // Map scene name
                string mapName = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_NAME, out mapName, string.Empty))
                {
                    _startingMapId = mapName;
                }

                // Channel Id
                string channelId = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CHANNEL_ID, out channelId, string.Empty))
                {
                    MapNetworkManager.ChannelId = channelId;
                }

                // Instance Id
                string instanceId = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ID, out instanceId, string.Empty))
                {
                    MapNetworkManager.MapInstanceId = instanceId;
                }

                // Instance Warp Position
                float instancePosX, instancePosY, instancePosZ;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_X, out instancePosX, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_Y, out instancePosY, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_Z, out instancePosZ, 0f))
                {
                    MapNetworkManager.MapInstanceWarpToPosition = new Vector3(instancePosX, instancePosY, instancePosZ);
                }

                // Instance Warp Override Rotation, Instance Warp Rotation
                MapNetworkManager.MapInstanceWarpOverrideRotation = ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_INSTANCE_OVERRIDE_ROTATION);
                float instanceRotX, instanceRotY, instanceRotZ;
                if (MapNetworkManager.MapInstanceWarpOverrideRotation &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_X, out instanceRotX, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_Y, out instanceRotY, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_Z, out instanceRotZ, 0f))
                {
                    MapNetworkManager.MapInstanceWarpToRotation = new Vector3(instanceRotX, instanceRotY, instanceRotZ);
                }

                // Allocate map server
                bool isAllocate = false;
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_ALLOCATE))
                {
                    MapNetworkManager.IsAllocate = true;
                    isAllocate = true;
                }

                if (!useCustomDatabaseClient)
                {
                    // Database network address
                    string databaseManagerAddress;
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_ADDRESS, out databaseManagerAddress, databaseNetworkManager.networkAddress))
                    {
                        databaseNetworkManager.networkAddress = databaseManagerAddress;
                    }
                    else if (!string.IsNullOrEmpty(currentServerConfig.databaseManagerAddress))
                    {
                        databaseNetworkManager.networkAddress = databaseManagerAddress = currentServerConfig.databaseManagerAddress;
                    }
                    currentServerConfig.databaseManagerAddress = databaseManagerAddress;

                    // Central network port
                    int databaseManagerPort;
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_PORT, out databaseManagerPort, databaseNetworkManager.networkPort))
                    {
                        databaseNetworkManager.networkPort = databaseManagerPort;
                    }
                    else if (currentServerConfig.databaseManagerPort.HasValue)
                    {
                        databaseNetworkManager.networkPort = databaseManagerPort = currentServerConfig.databaseManagerPort.Value;
                    }
                    currentServerConfig.databaseManagerPort = databaseManagerPort;
                }

                LogFileName = "Log";
                bool startLog = false;

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_DATABASE_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += "Database";
                    startLog = true;
                    _startingDatabaseServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_CENTRAL_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += "Central";
                    startLog = true;
                    _startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SPAWN_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += "MapSpawn";
                    startLog = true;
                    _startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += $"Map({mapName})-Ch({channelId})-Alloc({isAllocate})-Instance({instanceId})";
                    startLog = true;
                    _startingMapServer = true;
                }

                if ((_startingDatabaseServer || _startingCentralServer || _startingMapSpawnServer || _startingMapServer) && !configFileFound)
                {
                    ConfigManager.WriteServerConfigIfNotExisted(currentServerConfig);
                }

                if (startLog)
                {
                    EnableLogger(LogFileName);
                }
            }
            else
            {
                if (!useCustomDatabaseClient)
                    databaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);

                if (startDatabaseOnAwake)
                    _startingDatabaseServer = true;

                if (startCentralOnAwake)
                    _startingCentralServer = true;

                if (startMapSpawnOnAwake)
                    _startingMapSpawnServer = true;

                if (startMapOnAwake)
                {
                    // If run map-server, don't load home scene (home scene load in `Game Instance`)
                    _startingMapId = startingMap.Id;
                    _startingMapServer = true;
                }
            }
#endif
        }

        private void OnDestroy()
        {
            Application.wantsToQuit -= Application_wantsToQuit;
        }

        private bool Application_wantsToQuit()
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (MapNetworkManager != null && MapNetworkManager.IsServer && !MapNetworkManager.ReadyToQuit)
            {
                Logging.Log("[MapNetworkManager] still proceeding before quit.");
                MapNetworkManager.ProceedBeforeQuit();
                return false;
            }
            if (databaseNetworkManager != null && databaseNetworkManager.IsServer && !databaseNetworkManager.ReadyToQuit)
            {
                Logging.Log("[DatabaseNetworkManager] still proceeding before quit.");
                databaseNetworkManager.ProceedBeforeQuit();
                return false;
            }
#endif
            return true;
        }

        public void EnableLogger(string fileName)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            CacheLogGUI.SetupLogger(fileName);
            CacheLogGUI.enabled = true;
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private void OnGameDataLoaded()
        {
            GameInstance.LoadHomeScenePreventions[nameof(MMOServerInstance)] = false;

            if (_startingCentralServer ||
                _startingMapSpawnServer ||
                _startingMapServer)
            {
                // Allow to test in editor, so still load home scene in editor
                GameInstance.LoadHomeScenePreventions[nameof(MMOServerInstance)] = !Application.isEditor || _startingMapServer;
            }

            StartServers();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private async void StartServers()
        {
            // Wait a frame to make sure it will prepare servers' transports properly
            await UniTask.NextFrame();

            // Prepare guild data for guild database manager
            IDatabaseCache currentDatabaseCache = databaseNetworkManager.GetComponentInChildren<IDatabaseCache>();
            if (currentDatabaseCache == null)
                currentDatabaseCache = databaseNetworkManager.gameObject.AddComponent<LocalDatabaseCache>();
            if (disableDatabaseCaching)
                currentDatabaseCache = databaseNetworkManager.gameObject.AddComponent<DisabledDatabaseCache>();
            databaseNetworkManager.DatabaseCache = currentDatabaseCache;
            DatabaseNetworkManager.GuildMemberRoles = GameInstance.Singleton.SocialSystemSetting.GuildMemberRoles;
            DatabaseNetworkManager.GuildExpTree = GameInstance.Singleton.SocialSystemSetting.GuildExpTable.expTree;

            if (_startingDatabaseServer)
            {
                // Start database manager server
                StartDatabaseManagerServer();
            }

            if (_startingCentralServer)
            {
                // Start central server
                StartCentralServer();
            }

            if (_startingMapSpawnServer)
            {
                // Start map spawn server
                if (_spawningMaps != null && _spawningMaps.Count > 0)
                {
                    MapSpawnNetworkManager.spawningMaps = new List<BaseMapInfo>();
                    foreach (string spawningMapId in _spawningMaps)
                    {
                        if (!GameInstance.MapInfos.TryGetValue(spawningMapId, out BaseMapInfo tempMapInfo))
                            continue;
                        MapSpawnNetworkManager.spawningMaps.Add(tempMapInfo);
                    }
                }
                if (_spawningAllocateMaps != null && _spawningAllocateMaps.Count > 0)
                {
                    MapSpawnNetworkManager.spawningAllocateMaps = new List<SpawnAllocateMapData>();
                    foreach (SpawnAllocateMapByNameData spawningMap in _spawningAllocateMaps)
                    {
                        if (!GameInstance.MapInfos.TryGetValue(spawningMap.mapName, out BaseMapInfo tempMapInfo))
                            continue;
                        MapSpawnNetworkManager.spawningAllocateMaps.Add(new SpawnAllocateMapData()
                        {
                            mapInfo = tempMapInfo,
                            allocateAmount = spawningMap.allocateAmount,
                        });
                    }
                }
                StartMapSpawnServer();
            }

            if (_startingMapServer)
            {
                // Start map server
                BaseMapInfo tempMapInfo;
                if (!string.IsNullOrEmpty(_startingMapId) && GameInstance.MapInfos.TryGetValue(_startingMapId, out tempMapInfo))
                {
                    MapNetworkManager.Assets.addressableOnlineScene = tempMapInfo.AddressableScene;
#if !EXCLUDE_PREFAB_REFS
                    MapNetworkManager.Assets.onlineScene = tempMapInfo.Scene;
#endif
                    MapNetworkManager.SetMapInfo(tempMapInfo);
                }
                StartMapServer();
            }

            if (_startingCentralServer ||
                _startingMapSpawnServer ||
                _startingMapServer)
            {
                // Start database manager client, it will connect to database manager server
                // To request database functions
                StartDatabaseManagerClient();
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        #region Server functions
        public void StartCentralServer()
        {
            CentralNetworkManager.useWebSocket = UseWebSocket;
            CentralNetworkManager.webSocketSecure = WebSocketSecure;
            CentralNetworkManager.webSocketCertificateFilePath = WebSocketCertificateFilePath;
            CentralNetworkManager.webSocketCertificatePassword = WebSocketCertificatePassword;
            CentralNetworkManager.DatabaseClient = DatabaseClient;
            CentralNetworkManager.StartServer();
        }

        public void StartMapSpawnServer()
        {
            MapSpawnNetworkManager.StartServer();
        }

        public void StartMapServer()
        {
            MapNetworkManager.useWebSocket = UseWebSocket;
            MapNetworkManager.webSocketSecure = WebSocketSecure;
            MapNetworkManager.webSocketCertificateFilePath = WebSocketCertificateFilePath;
            MapNetworkManager.webSocketCertificatePassword = WebSocketCertificatePassword;
            MapNetworkManager.StartServer();
        }

        public void StartDatabaseManagerServer()
        {
            if (!useCustomDatabaseClient)
                databaseNetworkManager.StartServer();
        }

        public void StartDatabaseManagerClient()
        {
            if (!useCustomDatabaseClient)
                databaseNetworkManager.StartClient();
        }
        #endregion
#endif
    }
}
