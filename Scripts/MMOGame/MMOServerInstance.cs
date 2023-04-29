using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.IO;
using LiteNetLibManager;
using UnityEngine;
using Newtonsoft.Json;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(LogGUI))]
    [DefaultExecutionOrder(-899)]
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

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapSpawnNetworkManager MapSpawnNetworkManager { get { return mapSpawnNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public IDatabaseClient DatabaseClient
        {
            get
            {
                if (!useCustomDatabaseClient)
                    return databaseNetworkManager;
                else
                    return customDatabaseClient;
            }
        }
        public bool UseWebSocket { get { return useWebSocket; } }
        public bool WebSocketSecure { get { return webSocketSecure; } }
        public string WebSocketCertificateFilePath { get { return webSocketCertPath; } }
        public string WebSocketCertificatePassword { get { return webSocketCertPassword; } }

        private LogGUI cacheLogGUI;
        public LogGUI CacheLogGUI
        {
            get
            {
                if (cacheLogGUI == null)
                    cacheLogGUI = GetComponent<LogGUI>();
                return cacheLogGUI;
            }
        }

        [Header("Running In Editor")]
        public bool startCentralOnAwake;
        public bool startMapSpawnOnAwake;
        public bool startDatabaseOnAwake;
        public bool startMapOnAwake;
        public BaseMapInfo startingMap;
        public int databaseOptionIndex;
        public bool databaseDisableCacheReading;

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private List<string> spawningMapIds;
        private string startingMapId;
        private bool startingCentralServer;
        private bool startingMapSpawnServer;
        private bool startingMapServer;
        private bool startingDatabaseServer;
#endif
        private IDatabaseClient customDatabaseClient;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

            // Setup custom database client
            if (customDatabaseClientSource == null)
                customDatabaseClientSource = gameObject;
            customDatabaseClient = customDatabaseClientSource.GetComponent<IDatabaseClient>();

            CacheLogGUI.enabled = false;
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            GameInstance gameInstance = FindObjectOfType<GameInstance>();
            gameInstance.onGameDataLoaded = OnGameDataLoaded;

            if (!Application.isEditor)
            {
                // Json file read
                bool configFileFound = false;
                string configFolder = "./Config";
                string configFilePath = configFolder + "/serverConfig.json";
                Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
                Logging.Log(ToString(), "Reading config file from " + configFilePath);
                if (File.Exists(configFilePath))
                {
                    // Read config file
                    Logging.Log(ToString(), "Found config file");
                    string dataAsJson = File.ReadAllText(configFilePath);
                    jsonConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataAsJson);
                    configFileFound = true;
                }

                // Prepare data
                string[] args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

                // Database option index
                bool useCustomDatabaseClient = this.useCustomDatabaseClient = false;
                if (customDatabaseClient != null && customDatabaseClient as UnityEngine.Object != null)
                {
                    if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT, out useCustomDatabaseClient, this.useCustomDatabaseClient))
                    {
                        this.useCustomDatabaseClient = useCustomDatabaseClient;
                    }
                    else if (ConfigReader.IsArgsProvided(args, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT))
                    {
                        this.useCustomDatabaseClient = true;
                    }
                }
                jsonConfig[ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT] = useCustomDatabaseClient;

                int dbOptionIndex;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0))
                {
                    if (!useCustomDatabaseClient)
                        databaseNetworkManager.SetDatabaseByOptionIndex(dbOptionIndex);
                }
                jsonConfig[ProcessArguments.CONFIG_DATABASE_OPTION_INDEX] = dbOptionIndex;

                // Database disable cache reading or not?
                bool databaseDisableCacheReading = this.databaseDisableCacheReading = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_DISABLE_CACHE_READING, out databaseDisableCacheReading, this.databaseDisableCacheReading))
                {
                    this.databaseDisableCacheReading = databaseDisableCacheReading;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DATABASE_DISABLE_CACHE_READING))
                {
                    this.databaseDisableCacheReading = true;
                }
                jsonConfig[ProcessArguments.CONFIG_DATABASE_DISABLE_CACHE_READING] = databaseDisableCacheReading;

                // Use Websocket or not?
                bool useWebSocket = this.useWebSocket = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_USE_WEB_SOCKET, out useWebSocket, this.useWebSocket))
                {
                    this.useWebSocket = useWebSocket;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_USE_WEB_SOCKET))
                {
                    this.useWebSocket = true;
                }
                jsonConfig[ProcessArguments.CONFIG_USE_WEB_SOCKET] = useWebSocket;

                // Is websocket running in secure mode or not?
                bool webSocketSecure = this.webSocketSecure = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_WEB_SOCKET_SECURE, out webSocketSecure, this.webSocketSecure))
                {
                    this.webSocketSecure = webSocketSecure;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_WEB_SOCKET_SECURE))
                {
                    this.webSocketSecure = true;
                }
                jsonConfig[ProcessArguments.CONFIG_WEB_SOCKET_SECURE] = webSocketSecure;

                // Where is the certification file path?
                string webSocketCertPath;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath))
                {
                    this.webSocketCertPath = webSocketCertPath;
                }
                jsonConfig[ProcessArguments.CONFIG_WEB_SOCKET_CERT_PATH] = webSocketCertPath;

                // What is the certification password?
                string webSocketCertPassword;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword))
                {
                    this.webSocketCertPassword = webSocketCertPassword;
                }
                jsonConfig[ProcessArguments.CONFIG_WEB_SOCKET_CERT_PASSWORD] = webSocketCertPassword;

                // Central network address
                string centralNetworkAddress;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress))
                {
                    mapSpawnNetworkManager.clusterServerAddress = centralNetworkAddress;
                    mapNetworkManager.clusterServerAddress = centralNetworkAddress;
                }
                jsonConfig[ProcessArguments.CONFIG_CENTRAL_ADDRESS] = centralNetworkAddress;

                // Central network port
                int centralNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort))
                {
                    centralNetworkManager.networkPort = centralNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_CENTRAL_PORT] = centralNetworkPort;

                // Central max connections
                int centralMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections))
                {
                    centralNetworkManager.maxConnections = centralMaxConnections;
                }
                jsonConfig[ProcessArguments.CONFIG_CENTRAL_MAX_CONNECTIONS] = centralMaxConnections;

                // Central network port
                int clusterNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort))
                {
                    centralNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapSpawnNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapNetworkManager.clusterServerPort = clusterNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_CLUSTER_PORT] = clusterNetworkPort;

                // Machine network address, will be set to map spawn / map / chat
                string machineNetworkAddress;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress))
                {
                    mapSpawnNetworkManager.machineAddress = machineNetworkAddress;
                    mapNetworkManager.machineAddress = machineNetworkAddress;
                }
                jsonConfig[ProcessArguments.CONFIG_MACHINE_ADDRESS] = machineNetworkAddress;

                // Map spawn network port
                int mapSpawnNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort))
                {
                    mapSpawnNetworkManager.networkPort = mapSpawnNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_SPAWN_PORT] = mapSpawnNetworkPort;

                // Map spawn exe path
                string spawnExePath;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath))
                {
                    mapSpawnNetworkManager.exePath = spawnExePath;
                }
                if (!File.Exists(spawnExePath))
                {
                    spawnExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    mapSpawnNetworkManager.exePath = spawnExePath;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_EXE_PATH] = spawnExePath;

                // Map spawn in batch mode
                bool notSpawnInBatchMode = mapSpawnNetworkManager.notSpawnInBatchMode = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_NOT_SPAWN_IN_BATCH_MODE, out notSpawnInBatchMode, mapSpawnNetworkManager.notSpawnInBatchMode))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_NOT_SPAWN_IN_BATCH_MODE))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = true;
                }
                jsonConfig[ProcessArguments.CONFIG_NOT_SPAWN_IN_BATCH_MODE] = notSpawnInBatchMode;

                // Map spawn start port
                int spawnStartPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort))
                {
                    mapSpawnNetworkManager.startPort = spawnStartPort;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_START_PORT] = spawnStartPort;

                // Spawn maps
                List<string> defaultSpawnMapIds = new List<string>();
                foreach (BaseMapInfo mapInfo in mapSpawnNetworkManager.spawningMaps)
                {
                    if (mapInfo != null)
                        defaultSpawnMapIds.Add(mapInfo.Id);
                }
                List<string> spawnMapIds;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds))
                {
                    spawningMapIds = spawnMapIds;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_MAPS] = spawnMapIds;

                // Map network port
                int mapNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_PORT, out mapNetworkPort, mapNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_PORT, out mapNetworkPort, mapNetworkManager.networkPort))
                {
                    mapNetworkManager.networkPort = mapNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_PORT] = mapNetworkPort;

                // Map max connections
                int mapMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_MAX_CONNECTIONS, out mapMaxConnections, mapNetworkManager.maxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_MAX_CONNECTIONS, out mapMaxConnections, mapNetworkManager.maxConnections))
                {
                    mapNetworkManager.maxConnections = mapMaxConnections;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_MAX_CONNECTIONS] = mapMaxConnections;

                // Map scene name
                string mapId = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_ID, out mapId, string.Empty))
                {
                    startingMapId = mapId;
                }

                // Instance Id
                string instanceId = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ID, out instanceId, string.Empty))
                {
                    mapNetworkManager.MapInstanceId = instanceId;
                }

                // Instance Warp Position
                float instancePosX, instancePosY, instancePosZ;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_X, out instancePosX, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_Y, out instancePosY, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_Z, out instancePosZ, 0f))
                {
                    mapNetworkManager.MapInstanceWarpToPosition = new Vector3(instancePosX, instancePosY, instancePosZ);
                }

                // Instance Warp Override Rotation, Instance Warp Rotation
                mapNetworkManager.MapInstanceWarpOverrideRotation = ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_INSTANCE_OVERRIDE_ROTATION);
                float instanceRotX, instanceRotY, instanceRotZ;
                if (mapNetworkManager.MapInstanceWarpOverrideRotation &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_X, out instanceRotX, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_Y, out instanceRotY, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_Z, out instanceRotZ, 0f))
                {
                    mapNetworkManager.MapInstanceWarpToRotation = new Vector3(instanceRotX, instanceRotY, instanceRotZ);
                }

                if (!useCustomDatabaseClient)
                {
                    // Database network address
                    string databaseNetworkAddress;
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress) ||
                        ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress))
                    {
                        databaseNetworkManager.networkAddress = databaseNetworkAddress;
                    }
                    jsonConfig[ProcessArguments.CONFIG_DATABASE_ADDRESS] = databaseNetworkAddress;

                    // Database network port
                    int databaseNetworkPort;
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort) ||
                        ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort))
                    {
                        databaseNetworkManager.networkPort = databaseNetworkPort;
                    }
                    jsonConfig[ProcessArguments.CONFIG_DATABASE_PORT] = databaseNetworkPort;
                }

                string logFileName = "Log";
                bool startLog = false;

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_DATABASE_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Database";
                    startLog = true;
                    startingDatabaseServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_CENTRAL_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Central";
                    startLog = true;
                    startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SPAWN_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "MapSpawn";
                    startLog = true;
                    startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Map(" + mapId + ") Instance(" + instanceId + ")";
                    startLog = true;
                    startingMapServer = true;
                }

                if (startingDatabaseServer || startingCentralServer || startingMapSpawnServer || startingMapServer)
                {
                    if (!configFileFound)
                    {
                        // Write config file
                        Logging.Log(ToString(), "Not found config file, creating a new one");
                        if (!Directory.Exists(configFolder))
                            Directory.CreateDirectory(configFolder);
                        File.WriteAllText(configFilePath, JsonConvert.SerializeObject(jsonConfig, Formatting.Indented));
                    }
                }

                if (startLog)
                {
                    CacheLogGUI.SetupLogger(logFileName);
                    CacheLogGUI.enabled = true;
                }
            }
            else
            {
                if (!useCustomDatabaseClient)
                {
                    databaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);
                    databaseNetworkManager.DisableCacheReading = databaseDisableCacheReading;
                }

                if (startDatabaseOnAwake)
                    startingDatabaseServer = true;

                if (startCentralOnAwake)
                    startingCentralServer = true;

                if (startMapSpawnOnAwake)
                    startingMapSpawnServer = true;

                if (startMapOnAwake)
                {
                    // If run map-server, don't load home scene (home scene load in `Game Instance`)
                    startingMapId = startingMap.Id;
                    startingMapServer = true;
                }
            }
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void OnGameDataLoaded()
        {
            databaseNetworkManager.DatabaseCache = new LocalDatabaseCache();
            DatabaseNetworkManager.GuildMemberRoles = GameInstance.Singleton.SocialSystemSetting.GuildMemberRoles;
            DatabaseNetworkManager.GuildExpTree = GameInstance.Singleton.SocialSystemSetting.GuildExpTree;

            if (startingDatabaseServer)
            {
                // Start database manager server
                StartDatabaseManagerServer();
            }

            if (startingCentralServer)
            {
                // Start central server
                StartCentralServer();
            }

            if (startingMapSpawnServer)
            {
                // Start map spawn server
                if (spawningMapIds != null && spawningMapIds.Count > 0)
                {
                    mapSpawnNetworkManager.spawningMaps = new List<BaseMapInfo>();
                    BaseMapInfo tempMapInfo;
                    foreach (string spawningMapId in spawningMapIds)
                    {
                        if (GameInstance.MapInfos.TryGetValue(spawningMapId, out tempMapInfo))
                            mapSpawnNetworkManager.spawningMaps.Add(tempMapInfo);
                    }
                }
                StartMapSpawnServer();
            }

            if (startingMapServer)
            {
                // Start map server
                BaseMapInfo tempMapInfo;
                if (!string.IsNullOrEmpty(startingMapId) && GameInstance.MapInfos.TryGetValue(startingMapId, out tempMapInfo))
                {
                    mapNetworkManager.Assets.onlineScene.SceneName = tempMapInfo.Scene.SceneName;
                    mapNetworkManager.SetMapInfo(tempMapInfo);
                }
                StartMapServer();
            }

            GameInstance gameInstance = FindObjectOfType<GameInstance>();
            gameInstance.LoadHomeScenePreventions[nameof(MMOServerInstance)] = false;

            if (startingCentralServer ||
                startingMapSpawnServer ||
                startingMapServer)
            {
                // Start database manager client, it will connect to database manager server
                // To request database functions
                gameInstance.LoadHomeScenePreventions[nameof(MMOServerInstance)] = !Application.isEditor || startingMapServer;
                StartDatabaseManagerClient();
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        #region Server functions
        public void StartCentralServer()
        {
            CentralNetworkManager.useWebSocket = UseWebSocket;
            CentralNetworkManager.webSocketSecure = WebSocketSecure;
            CentralNetworkManager.webSocketCertificateFilePath = WebSocketCertificateFilePath;
            CentralNetworkManager.webSocketCertificatePassword = WebSocketCertificatePassword;
            centralNetworkManager.DbServiceClient = DatabaseClient;
            centralNetworkManager.DataManager = new CentralServerDataManager();
            CentralNetworkManager.StartServer();
        }

        public void StartMapSpawnServer()
        {
            mapSpawnNetworkManager.StartServer();
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
