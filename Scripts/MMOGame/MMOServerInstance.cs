using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.IO;
using LiteNetLibManager;
using UnityEngine;
using MiniJSON;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(LogGUI))]
    [DefaultExecutionOrder(-899)]
    public class MMOServerInstance : MonoBehaviour
    {
        public static MMOServerInstance Singleton { get; protected set; }

        public const string CONFIG_DATABASE_OPTION_INDEX = "databaseOptionIndex";
        public const string ARG_DATABASE_OPTION_INDEX = "-" + CONFIG_DATABASE_OPTION_INDEX;
        public const string CONFIG_DATABASE_DISABLE_CACHE_READING = "databaseDisableCacheReading";
        public const string ARG_DATABASE_DISABLE_CACHE_READING = "-" + CONFIG_DATABASE_DISABLE_CACHE_READING;
        public const string CONFIG_CENTRAL_ADDRESS = "centralAddress";
        public const string ARG_CENTRAL_ADDRESS = "-" + CONFIG_CENTRAL_ADDRESS;
        public const string CONFIG_CENTRAL_PORT = "centralPort";
        public const string ARG_CENTRAL_PORT = "-" + CONFIG_CENTRAL_PORT;
        public const string CONFIG_CENTRAL_MAX_CONNECTIONS = "centralMaxConnections";
        public const string ARG_CENTRAL_MAX_CONNECTIONS = "-" + CONFIG_CENTRAL_MAX_CONNECTIONS;
        public const string CONFIG_CLUSTER_PORT = "clusterPort";
        public const string ARG_CLUSTER_PORT = "-" + CONFIG_CLUSTER_PORT;
        public const string CONFIG_MACHINE_ADDRESS = "machineAddress";
        public const string ARG_MACHINE_ADDRESS = "-" + CONFIG_MACHINE_ADDRESS;
        public const string CONFIG_USE_WEB_SOCKET = "useWebSocket";
        public const string ARG_USE_WEB_SOCKET = "-" + CONFIG_USE_WEB_SOCKET;
        public const string CONFIG_WEB_SOCKET_SECURE = "webSocketSecure";
        public const string ARG_WEB_SOCKET_SECURE = "-" + CONFIG_WEB_SOCKET_SECURE;
        public const string CONFIG_WEB_SOCKET_CERT_PATH = "webSocketCertPath";
        public const string ARG_WEB_SOCKET_CERT_PATH = "-" + CONFIG_WEB_SOCKET_CERT_PATH;
        public const string CONFIG_WEB_SOCKET_CERT_PASSWORD = "webSocketCertPassword";
        public const string ARG_WEB_SOCKET_CERT_PASSWORD = "-" + CONFIG_WEB_SOCKET_CERT_PASSWORD;
        // Map spawn server
        public const string CONFIG_MAP_SPAWN_PORT = "mapSpawnPort";
        public const string ARG_MAP_SPAWN_PORT = "-" + CONFIG_MAP_SPAWN_PORT;
        public const string CONFIG_SPAWN_EXE_PATH = "spawnExePath";
        public const string ARG_SPAWN_EXE_PATH = "-" + CONFIG_SPAWN_EXE_PATH;
        public const string CONFIG_NOT_SPAWN_IN_BATCH_MODE = "notSpawnInBatchMode";
        public const string ARG_NOT_SPAWN_IN_BATCH_MODE = "-" + CONFIG_NOT_SPAWN_IN_BATCH_MODE;
        public const string CONFIG_SPAWN_START_PORT = "spawnStartPort";
        public const string ARG_SPAWN_START_PORT = "-" + CONFIG_SPAWN_START_PORT;
        public const string CONFIG_SPAWN_MAPS = "spawnMaps";
        public const string ARG_SPAWN_MAPS = "-" + CONFIG_SPAWN_MAPS;
        // Map server
        public const string CONFIG_MAP_PORT = "mapPort";
        public const string ARG_MAP_PORT = "-" + CONFIG_MAP_PORT;
        public const string CONFIG_MAP_MAX_CONNECTIONS = "mapMaxConnections";
        public const string ARG_MAP_MAX_CONNECTIONS = "-" + CONFIG_MAP_MAX_CONNECTIONS;
        public const string CONFIG_MAP_ID = "mapId";
        public const string ARG_MAP_ID = "-" + CONFIG_MAP_ID;
        public const string CONFIG_INSTANCE_ID = "instanceId";
        public const string ARG_INSTANCE_ID = "-" + CONFIG_INSTANCE_ID;
        public const string CONFIG_INSTANCE_POSITION_X = "instancePositionX";
        public const string ARG_INSTANCE_POSITION_X = "-" + CONFIG_INSTANCE_POSITION_X;
        public const string CONFIG_INSTANCE_POSITION_Y = "instancePositionY";
        public const string ARG_INSTANCE_POSITION_Y = "-" + CONFIG_INSTANCE_POSITION_Y;
        public const string CONFIG_INSTANCE_POSITION_Z = "instancePositionZ";
        public const string ARG_INSTANCE_POSITION_Z = "-" + CONFIG_INSTANCE_POSITION_Z;
        public const string CONFIG_INSTANCE_OVERRIDE_ROTATION = "instanceOverrideRotation";
        public const string ARG_INSTANCE_OVERRIDE_ROTATION = "-" + CONFIG_INSTANCE_OVERRIDE_ROTATION;
        public const string CONFIG_INSTANCE_ROTATION_X = "instanceRotationX";
        public const string ARG_INSTANCE_ROTATION_X = "-" + CONFIG_INSTANCE_ROTATION_X;
        public const string CONFIG_INSTANCE_ROTATION_Y = "instanceRotationY";
        public const string ARG_INSTANCE_ROTATION_Y = "-" + CONFIG_INSTANCE_ROTATION_Y;
        public const string CONFIG_INSTANCE_ROTATION_Z = "instanceRotationZ";
        public const string ARG_INSTANCE_ROTATION_Z = "-" + CONFIG_INSTANCE_ROTATION_Z;
        // Database manager server
        public const string CONFIG_DATABASE_ADDRESS = "databaseManagerAddress";
        public const string ARG_DATABASE_ADDRESS = "-" + CONFIG_DATABASE_ADDRESS;
        public const string CONFIG_DATABASE_PORT = "databaseManagerPort";
        public const string ARG_DATABASE_PORT = "-" + CONFIG_DATABASE_PORT;
        // Start servers
        public const string CONFIG_START_CENTRAL_SERVER = "startCentralServer";
        public const string ARG_START_CENTRAL_SERVER = "-" + CONFIG_START_CENTRAL_SERVER;
        public const string CONFIG_START_MAP_SPAWN_SERVER = "startMapSpawnServer";
        public const string ARG_START_MAP_SPAWN_SERVER = "-" + CONFIG_START_MAP_SPAWN_SERVER;
        public const string CONFIG_START_MAP_SERVER = "startMapServer";
        public const string ARG_START_MAP_SERVER = "-" + CONFIG_START_MAP_SERVER;
        public const string CONFIG_START_DATABASE_SERVER = "startDatabaseServer";
        public const string ARG_START_DATABASE_SERVER = "-" + CONFIG_START_DATABASE_SERVER;

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
        public IDatabaseClient DatabaseNetworkManager
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
        public string WebScoketCertificateFilePath { get { return webSocketCertPath; } }
        public string WebScoketCertificatePassword { get { return webSocketCertPassword; } }

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
                string configFolder = "./config";
                string configFilePath = configFolder + "/serverConfig.json";
                Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
                Logging.Log(ToString(), "Reading config file from " + configFilePath);
                if (File.Exists(configFilePath))
                {
                    // Read config file
                    Logging.Log(ToString(), "Found config file");
                    string dataAsJson = File.ReadAllText(configFilePath);
                    jsonConfig = Json.Deserialize(dataAsJson) as Dictionary<string, object>;
                    configFileFound = true;
                }

                // Prepare data
                string[] args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

                // Database option index
                int dbOptionIndex;
                if (ConfigReader.ReadArgs(args, ARG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0))
                {
                    if (!useCustomDatabaseClient)
                        databaseNetworkManager.SetDatabaseByOptionIndex(dbOptionIndex);
                    jsonConfig[CONFIG_DATABASE_OPTION_INDEX] = dbOptionIndex;
                }
                jsonConfig[CONFIG_DATABASE_OPTION_INDEX] = dbOptionIndex;

                // Database disable cache reading or not?
                bool databaseDisableCacheReading = this.databaseDisableCacheReading = false;
                if (ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_DISABLE_CACHE_READING, out databaseDisableCacheReading, this.databaseDisableCacheReading))
                {
                    this.databaseDisableCacheReading = databaseDisableCacheReading;
                    jsonConfig[CONFIG_DATABASE_DISABLE_CACHE_READING] = databaseDisableCacheReading;
                }
                else if (ConfigReader.IsArgsProvided(args, ARG_DATABASE_DISABLE_CACHE_READING))
                {
                    this.databaseDisableCacheReading = true;
                }
                jsonConfig[CONFIG_DATABASE_DISABLE_CACHE_READING] = databaseDisableCacheReading;

                // Use Websocket or not?
                bool useWebSocket = this.useWebSocket = false;
                if (ConfigReader.ReadConfigs(jsonConfig, CONFIG_USE_WEB_SOCKET, out useWebSocket, this.useWebSocket))
                {
                    this.useWebSocket = useWebSocket;
                    jsonConfig[CONFIG_USE_WEB_SOCKET] = useWebSocket;
                }
                else if (ConfigReader.IsArgsProvided(args, ARG_USE_WEB_SOCKET))
                {
                    this.useWebSocket = true;
                }
                jsonConfig[CONFIG_USE_WEB_SOCKET] = useWebSocket;

                // Is websocket running in secure mode or not?
                bool webSocketSecure = this.webSocketSecure = false;
                if (ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_SECURE, out webSocketSecure, this.webSocketSecure))
                {
                    this.webSocketSecure = webSocketSecure;
                    jsonConfig[CONFIG_WEB_SOCKET_SECURE] = webSocketSecure;
                }
                else if (ConfigReader.IsArgsProvided(args, ARG_WEB_SOCKET_SECURE))
                {
                    this.webSocketSecure = true;
                }
                jsonConfig[CONFIG_WEB_SOCKET_SECURE] = webSocketSecure;

                // Where is the certification file path?
                string webSocketCertPath;
                if (ConfigReader.ReadArgs(args, ARG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath))
                {
                    this.webSocketCertPath = webSocketCertPath;
                    jsonConfig[CONFIG_WEB_SOCKET_CERT_PATH] = webSocketCertPath;
                }
                jsonConfig[CONFIG_WEB_SOCKET_CERT_PATH] = webSocketCertPath;

                // What is the certification password?
                string webSocketCertPassword;
                if (ConfigReader.ReadArgs(args, ARG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword))
                {
                    this.webSocketCertPassword = webSocketCertPassword;
                    jsonConfig[CONFIG_WEB_SOCKET_CERT_PASSWORD] = webSocketCertPassword;
                }
                jsonConfig[CONFIG_WEB_SOCKET_CERT_PASSWORD] = webSocketCertPassword;

                // Central network address
                string centralNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress))
                {
                    mapSpawnNetworkManager.clusterServerAddress = centralNetworkAddress;
                    mapNetworkManager.clusterServerAddress = centralNetworkAddress;
                    jsonConfig[CONFIG_CENTRAL_ADDRESS] = centralNetworkAddress;
                }
                jsonConfig[CONFIG_CENTRAL_ADDRESS] = centralNetworkAddress;

                // Central network port
                int centralNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort))
                {
                    centralNetworkManager.networkPort = centralNetworkPort;
                    jsonConfig[CONFIG_CENTRAL_PORT] = centralNetworkPort;
                }
                jsonConfig[CONFIG_CENTRAL_PORT] = centralNetworkPort;

                // Central max connections
                int centralMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections))
                {
                    centralNetworkManager.maxConnections = centralMaxConnections;
                    jsonConfig[CONFIG_CENTRAL_MAX_CONNECTIONS] = centralMaxConnections;
                }
                jsonConfig[CONFIG_CENTRAL_MAX_CONNECTIONS] = centralMaxConnections;

                // Central network port
                int clusterNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort))
                {
                    centralNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapSpawnNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapNetworkManager.clusterServerPort = clusterNetworkPort;
                    jsonConfig[CONFIG_CLUSTER_PORT] = clusterNetworkPort;
                }
                jsonConfig[CONFIG_CLUSTER_PORT] = clusterNetworkPort;

                // Machine network address, will be set to map spawn / map / chat
                string machineNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress))
                {
                    mapSpawnNetworkManager.machineAddress = machineNetworkAddress;
                    mapNetworkManager.machineAddress = machineNetworkAddress;
                    jsonConfig[CONFIG_MACHINE_ADDRESS] = machineNetworkAddress;
                }
                jsonConfig[CONFIG_MACHINE_ADDRESS] = machineNetworkAddress;

                // Map spawn network port
                int mapSpawnNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort))
                {
                    mapSpawnNetworkManager.networkPort = mapSpawnNetworkPort;
                    jsonConfig[CONFIG_MAP_SPAWN_PORT] = mapSpawnNetworkPort;
                }
                jsonConfig[CONFIG_MAP_SPAWN_PORT] = mapSpawnNetworkPort;

                // Map spawn exe path
                string spawnExePath;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath))
                {
                    mapSpawnNetworkManager.exePath = spawnExePath;
                    jsonConfig[CONFIG_SPAWN_EXE_PATH] = spawnExePath;
                }
                jsonConfig[CONFIG_SPAWN_EXE_PATH] = spawnExePath;

                // Map spawn in batch mode
                bool notSpawnInBatchMode = mapSpawnNetworkManager.notSpawnInBatchMode = false;
                if (ConfigReader.ReadConfigs(jsonConfig, CONFIG_NOT_SPAWN_IN_BATCH_MODE, out notSpawnInBatchMode, mapSpawnNetworkManager.notSpawnInBatchMode))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode;
                    jsonConfig[CONFIG_NOT_SPAWN_IN_BATCH_MODE] = notSpawnInBatchMode;
                }
                else if (ConfigReader.IsArgsProvided(args, ARG_NOT_SPAWN_IN_BATCH_MODE))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = true;
                }
                jsonConfig[CONFIG_NOT_SPAWN_IN_BATCH_MODE] = notSpawnInBatchMode;

                // Map spawn start port
                int spawnStartPort;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort))
                {
                    mapSpawnNetworkManager.startPort = spawnStartPort;
                    jsonConfig[CONFIG_SPAWN_START_PORT] = spawnStartPort;
                }
                jsonConfig[CONFIG_SPAWN_START_PORT] = spawnStartPort;

                // Spawn maps
                List<string> defaultSpawnMapIds = new List<string>();
                foreach (BaseMapInfo mapInfo in mapSpawnNetworkManager.spawningMaps)
                {
                    if (mapInfo != null)
                        defaultSpawnMapIds.Add(mapInfo.Id);
                }
                List<string> spawnMapIds;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_MAPS, out spawnMapIds, defaultSpawnMapIds))
                {
                    spawningMapIds = spawnMapIds;
                    jsonConfig[CONFIG_SPAWN_MAPS] = spawnMapIds;
                }
                jsonConfig[CONFIG_SPAWN_MAPS] = spawnMapIds;

                // Map network port
                int mapNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_MAP_PORT, out mapNetworkPort, mapNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_PORT, out mapNetworkPort, mapNetworkManager.networkPort))
                {
                    mapNetworkManager.networkPort = mapNetworkPort;
                    jsonConfig[CONFIG_MAP_PORT] = mapNetworkPort;
                }
                jsonConfig[CONFIG_MAP_PORT] = mapNetworkPort;

                // Map max connections
                int mapMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_MAP_MAX_CONNECTIONS, out mapMaxConnections, mapNetworkManager.maxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_MAX_CONNECTIONS, out mapMaxConnections, mapNetworkManager.maxConnections))
                {
                    mapNetworkManager.maxConnections = mapMaxConnections;
                    jsonConfig[CONFIG_MAP_MAX_CONNECTIONS] = mapMaxConnections;
                }
                jsonConfig[CONFIG_MAP_MAX_CONNECTIONS] = mapMaxConnections;

                // Map scene name
                string mapId = string.Empty;
                if (ConfigReader.ReadArgs(args, ARG_MAP_ID, out mapId, string.Empty))
                {
                    startingMapId = mapId;
                }

                // Instance Id
                string instanceId = string.Empty;
                if (ConfigReader.ReadArgs(args, ARG_INSTANCE_ID, out instanceId, string.Empty))
                {
                    mapNetworkManager.MapInstanceId = instanceId;
                }

                // Instance Warp Position
                float instancePosX, instancePosY, instancePosZ;
                if (ConfigReader.ReadArgs(args, ARG_INSTANCE_POSITION_X, out instancePosX, 0f) &&
                    ConfigReader.ReadArgs(args, ARG_INSTANCE_POSITION_Y, out instancePosY, 0f) &&
                    ConfigReader.ReadArgs(args, ARG_INSTANCE_POSITION_Z, out instancePosZ, 0f))
                {
                    mapNetworkManager.MapInstanceWarpToPosition = new Vector3(instancePosX, instancePosY, instancePosZ);
                }

                // Instance Warp Override Rotation, Instance Warp Rotation
                mapNetworkManager.MapInstanceWarpOverrideRotation = ConfigReader.IsArgsProvided(args, ARG_INSTANCE_OVERRIDE_ROTATION);
                float instanceRotX, instanceRotY, instanceRotZ;
                if (mapNetworkManager.MapInstanceWarpOverrideRotation &&
                    ConfigReader.ReadArgs(args, ARG_INSTANCE_ROTATION_X, out instanceRotX, 0f) &&
                    ConfigReader.ReadArgs(args, ARG_INSTANCE_ROTATION_Y, out instanceRotY, 0f) &&
                    ConfigReader.ReadArgs(args, ARG_INSTANCE_ROTATION_Z, out instanceRotZ, 0f))
                {
                    mapNetworkManager.MapInstanceWarpToRotation = new Vector3(instanceRotX, instanceRotY, instanceRotZ);
                }

                if (!useCustomDatabaseClient)
                {
                    // Database network address
                    string databaseNetworkAddress;
                    if (ConfigReader.ReadArgs(args, ARG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress) ||
                        ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress))
                    {
                        databaseNetworkManager.networkAddress = databaseNetworkAddress;
                        jsonConfig[CONFIG_DATABASE_ADDRESS] = databaseNetworkAddress;
                    }
                    jsonConfig[CONFIG_DATABASE_ADDRESS] = databaseNetworkAddress;

                    // Database network port
                    int databaseNetworkPort;
                    if (ConfigReader.ReadArgs(args, ARG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort) ||
                        ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort))
                    {
                        if (!useCustomDatabaseClient)
                            databaseNetworkManager.networkPort = databaseNetworkPort;
                        jsonConfig[CONFIG_DATABASE_PORT] = databaseNetworkPort;
                    }
                    jsonConfig[CONFIG_DATABASE_PORT] = databaseNetworkPort;
                }

                string logFileName = "Log";
                bool startLog = false;

                if (ConfigReader.IsArgsProvided(args, ARG_START_DATABASE_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Database";
                    startLog = true;
                    startingDatabaseServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_CENTRAL_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Central";
                    startLog = true;
                    startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "MapSpawn";
                    startLog = true;
                    startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SERVER))
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
                        string jsonContent = string.Empty;
                        foreach (KeyValuePair<string, object> kv in jsonConfig)
                        {
                            if (!string.IsNullOrEmpty(jsonContent))
                            {
                                jsonContent += ",\n";
                            }
                            if (kv.Value is string)
                            {
                                jsonContent += $" \"{kv.Key}\": \"{kv.Value}\"";
                            }
                            if (kv.Value is byte || kv.Value is ushort || kv.Value is uint || kv.Value is ulong ||
                                kv.Value is sbyte || kv.Value is short || kv.Value is int || kv.Value is long)
                            {
                                jsonContent += $" \"{kv.Key}\": {kv.Value}";
                            }
                            if (kv.Value is bool)
                            {
                                jsonContent += $" \"{kv.Key}\": {kv.Value.ToString().ToLower()}";
                            }
                            if (kv.Value is List<string>)
                            {
                                jsonContent += $" \"{kv.Key}\": ";
                                string arrayContent = string.Empty;
                                foreach (string entry in (List<string>)kv.Value)
                                {
                                    if (!string.IsNullOrEmpty(arrayContent))
                                    {
                                        arrayContent += ",";
                                    }
                                    arrayContent += $"\"{entry}\"";
                                }
                                arrayContent = $"[{arrayContent}]";
                                jsonContent += arrayContent;
                            }
                        }
                        jsonContent = "{\n" + jsonContent + "\n}";
                        File.WriteAllText(configFilePath, jsonContent);
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
            CentralNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
            CentralNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;
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
            MapNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
            MapNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;
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
