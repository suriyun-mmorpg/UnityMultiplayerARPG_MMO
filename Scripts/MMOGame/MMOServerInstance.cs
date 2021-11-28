using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Security.Authentication;
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

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;
        [SerializeField]
        private bool webSocketSecure = false;
        [SerializeField]
        private SslProtocols webSocketSslProtocols = SslProtocols.Tls12;
        [SerializeField]
        private string webSocketCertPath = string.Empty;
        [SerializeField]
        private string webSocketCertPassword = string.Empty;

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapSpawnNetworkManager MapSpawnNetworkManager { get { return mapSpawnNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public DatabaseNetworkManager DatabaseNetworkManager { get { return databaseNetworkManager; } }
        public bool UseWebSocket { get { return useWebSocket; } }
        public bool WebSocketSecure { get { return webSocketSecure; } }
        public SslProtocols WebSocketSslProtocols { get { return webSocketSslProtocols; } }
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

#if UNITY_STANDALONE && !CLIENT_BUILD
        private List<string> spawningMapIds;
        private string startingMapId;
        private bool startingCentralServer;
        private bool startingMapSpawnServer;
        private bool startingMapServer;
        private bool startingDatabaseServer;
#endif

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

            CacheLogGUI.enabled = false;
#if UNITY_STANDALONE && !CLIENT_BUILD
            GameInstance gameInstance = FindObjectOfType<GameInstance>();
            gameInstance.onGameDataLoaded = OnGameDataLoaded;

            if (!Application.isEditor)
            {
                // Json file read
                string configFilePath = "./config/serverConfig.json";
                Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
                Logging.Log(ToString(), "Reading config file from " + configFilePath);
                if (File.Exists(configFilePath))
                {
                    Logging.Log(ToString(), "Found config file");
                    string dataAsJson = File.ReadAllText(configFilePath);
                    jsonConfig = Json.Deserialize(dataAsJson) as Dictionary<string, object>;
                }

                // Prepare data
                string[] args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

                // Database option index
                int dbOptionIndex;
                if (ConfigReader.ReadArgs(args, ARG_DATABASE_OPTION_INDEX, out dbOptionIndex, -1) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_OPTION_INDEX, out dbOptionIndex, -1))
                {
                    DatabaseNetworkManager.SetDatabaseByOptionIndex(dbOptionIndex);
                }

                // Active WebSockets
                bool useWebSocket = ConfigReader.IsArgsProvided(args, ARG_USE_WEB_SOCKET);
                if (useWebSocket || ConfigReader.ReadConfigs(jsonConfig, CONFIG_USE_WEB_SOCKET, out useWebSocket))
                {
                    this.useWebSocket = useWebSocket;
                }
                bool webSocketSecure = ConfigReader.IsArgsProvided(args, ARG_WEB_SOCKET_SECURE);
                if (webSocketSecure || ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_SECURE, out webSocketSecure))
                {
                    this.webSocketSecure = webSocketSecure;
                }
                string sslProtocols;
                if (ConfigReader.ReadArgs(args, ARG_WEB_SOCKET_CERT_PATH, out sslProtocols, string.Empty) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_CERT_PATH, out sslProtocols, string.Empty))
                {
                    if (!Enum.TryParse(sslProtocols, out webSocketSslProtocols))
                        webSocketSslProtocols = SslProtocols.Tls12;
                }
                string webSocketCertPath;
                if (ConfigReader.ReadArgs(args, ARG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, string.Empty) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, string.Empty))
                {
                    this.webSocketCertPath = webSocketCertPath;
                }
                string webSocketCertPassword;
                if (ConfigReader.ReadArgs(args, ARG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, string.Empty) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, string.Empty))
                {
                    this.webSocketCertPassword = webSocketCertPassword;
                }

                // Active WebSockets
                CentralNetworkManager.useWebSocket = UseWebSocket;
                CentralNetworkManager.webSocketSecure = WebSocketSecure;
                CentralNetworkManager.webSocketSslProtocols = WebSocketSslProtocols;
                CentralNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
                CentralNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;

                MapSpawnNetworkManager.useWebSocket = UseWebSocket;
                MapSpawnNetworkManager.webSocketSecure = WebSocketSecure;
                MapSpawnNetworkManager.webSocketSslProtocols = WebSocketSslProtocols;
                MapSpawnNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
                MapSpawnNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;

                MapNetworkManager.useWebSocket = UseWebSocket;
                MapNetworkManager.webSocketSecure = WebSocketSecure;
                MapNetworkManager.webSocketSslProtocols = WebSocketSslProtocols;
                MapNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
                MapNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;

                // Central network address
                string centralNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_ADDRESS, out centralNetworkAddress, "localhost") ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_ADDRESS, out centralNetworkAddress, "localhost"))
                {
                    mapSpawnNetworkManager.clusterServerAddress = centralNetworkAddress;
                    mapNetworkManager.clusterServerAddress = centralNetworkAddress;
                }

                // Central network port
                int centralNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_PORT, out centralNetworkPort, 6000) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_PORT, out centralNetworkPort, 6000))
                {
                    centralNetworkManager.networkPort = centralNetworkPort;
                }

                // Central max connections
                int centralMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, 1100) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, 1100))
                {
                    centralNetworkManager.maxConnections = centralMaxConnections;
                }

                // Central network port
                int clusterNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_CLUSTER_PORT, out clusterNetworkPort, 6010) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CLUSTER_PORT, out clusterNetworkPort, 6010))
                {
                    centralNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapSpawnNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapNetworkManager.clusterServerPort = clusterNetworkPort;
                }

                // Machine network address, will be set to map spawn / map / chat
                string machineNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_MACHINE_ADDRESS, out machineNetworkAddress, "localhost") ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MACHINE_ADDRESS, out machineNetworkAddress, "localhost"))
                {
                    mapSpawnNetworkManager.machineAddress = machineNetworkAddress;
                    mapNetworkManager.machineAddress = machineNetworkAddress;
                }

                // Map spawn network port
                int mapSpawnNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, 6001) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, 6001))
                {
                    mapSpawnNetworkManager.networkPort = mapSpawnNetworkPort;
                }

                // Map spawn exe path
                string spawnExePath;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_EXE_PATH, out spawnExePath, "./Build.exe") ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_EXE_PATH, out spawnExePath, "./Build.exe"))
                {
                    mapSpawnNetworkManager.exePath = spawnExePath;
                }

                // Map spawn in batch mode
                bool notSpawnInBatchMode = ConfigReader.IsArgsProvided(args, ARG_NOT_SPAWN_IN_BATCH_MODE);
                if (notSpawnInBatchMode || ConfigReader.ReadConfigs(jsonConfig, CONFIG_NOT_SPAWN_IN_BATCH_MODE, out notSpawnInBatchMode))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode;
                }

                // Map spawn start port
                int spawnStartPort;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_START_PORT, out spawnStartPort, 8001) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_START_PORT, out spawnStartPort, 8001))
                {
                    mapSpawnNetworkManager.startPort = spawnStartPort;
                }

                // Spawn maps
                List<string> spawnMapIds;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_MAPS, out spawnMapIds, new List<string>()) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_MAPS, out spawnMapIds, new List<string>()))
                {
                    spawningMapIds = spawnMapIds;
                }

                // Map network port
                int mapNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_MAP_PORT, out mapNetworkPort, 6002) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_PORT, out mapNetworkPort, 6002))
                {
                    mapNetworkManager.networkPort = mapNetworkPort;
                }

                // Map max connections
                int mapMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_MAP_MAX_CONNECTIONS, out mapMaxConnections, 1100) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_MAX_CONNECTIONS, out mapMaxConnections, 1100))
                {
                    mapNetworkManager.maxConnections = mapMaxConnections;
                }

                // Map scene name
                string mapId = string.Empty;
                if (ConfigReader.ReadArgs(args, ARG_MAP_ID, out mapId, string.Empty) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_ID, out mapId))
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

                // Database network address
                string databaseNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_DATABASE_ADDRESS, out databaseNetworkAddress, "localhost") ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_ADDRESS, out databaseNetworkAddress, "localhost"))
                {
                    databaseNetworkManager.networkAddress = databaseNetworkAddress;
                }

                // Database network port
                int databaseNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_DATABASE_PORT, out databaseNetworkPort, 6100) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_DATABASE_PORT, out databaseNetworkPort, 6100))
                {
                    databaseNetworkManager.networkPort = databaseNetworkPort;
                }

                string logFileName = "Log";
                bool startLog = false;
                bool tempStartServer;

                if (ConfigReader.IsArgsProvided(args, ARG_START_DATABASE_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_DATABASE_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Database";
                    startLog = true;
                    startingDatabaseServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_CENTRAL_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_CENTRAL_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Central";
                    startLog = true;
                    startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_MAP_SPAWN_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "MapSpawn";
                    startLog = true;
                    startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_MAP_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Map(" + mapId + ") Instance(" + instanceId + ")";
                    startLog = true;
                    startingMapServer = true;
                }

                if (startLog)
                {
                    CacheLogGUI.SetupLogger(logFileName);
                    CacheLogGUI.enabled = true;
                }
            }
            else
            {
                DatabaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);

                // Active WebSockets
                CentralNetworkManager.useWebSocket = UseWebSocket;
                CentralNetworkManager.webSocketSecure = WebSocketSecure;
                CentralNetworkManager.webSocketSslProtocols = WebSocketSslProtocols;
                CentralNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
                CentralNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;

                MapSpawnNetworkManager.useWebSocket = UseWebSocket;
                MapSpawnNetworkManager.webSocketSecure = WebSocketSecure;
                MapSpawnNetworkManager.webSocketSslProtocols = WebSocketSslProtocols;
                MapSpawnNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
                MapSpawnNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;

                MapNetworkManager.useWebSocket = UseWebSocket;
                MapNetworkManager.webSocketSecure = WebSocketSecure;
                MapNetworkManager.webSocketSslProtocols = WebSocketSslProtocols;
                MapNetworkManager.webSocketCertificateFilePath = WebScoketCertificateFilePath;
                MapNetworkManager.webSocketCertificatePassword = WebScoketCertificatePassword;

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

#if UNITY_STANDALONE && !CLIENT_BUILD
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

#if UNITY_STANDALONE && !CLIENT_BUILD
        #region Server functions
        public void StartCentralServer()
        {
            centralNetworkManager.StartServer();
        }

        public void StartMapSpawnServer()
        {
            mapSpawnNetworkManager.StartServer();
        }

        public void StartMapServer()
        {
            mapNetworkManager.StartServer();
        }

        public void StartDatabaseManagerServer()
        {
            DatabaseNetworkManager.StartServer();
        }

        public void StartDatabaseManagerClient()
        {
            DatabaseNetworkManager.StartClient();
        }
        #endregion
#endif
    }
}
