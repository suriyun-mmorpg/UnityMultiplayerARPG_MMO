using System;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using System.Net;
using System.Net.Security;
using System.IO;
using MiniJSON;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(LogGUI))]
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
        public const string CONFIG_MACHINE_ADDRESS = "machineAddress";
        public const string ARG_MACHINE_ADDRESS = "-" + CONFIG_MACHINE_ADDRESS;
        // Map spawn server
        public const string CONFIG_MAP_SPAWN_PORT = "mapSpawnPort";
        public const string ARG_MAP_SPAWN_PORT = "-" + CONFIG_MAP_SPAWN_PORT;
        public const string CONFIG_MAP_SPAWN_MAX_CONNECTIONS = "mapSpawnMaxConnections";
        public const string ARG_MAP_SPAWN_MAX_CONNECTIONS = "-" + CONFIG_MAP_SPAWN_MAX_CONNECTIONS;
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
        // Chat server
        public const string CONFIG_CHAT_PORT = "chatPort";
        public const string ARG_CHAT_PORT = "-" + CONFIG_CHAT_PORT;
        public const string CONFIG_CHAT_MAX_CONNECTIONS = "chatMaxConnections";
        public const string ARG_CHAT_MAX_CONNECTIONS = "-" + CONFIG_CHAT_MAX_CONNECTIONS;
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
        public const string CONFIG_START_CHAT_SERVER = "startChatServer";
        public const string ARG_START_CHAT_SERVER = "-" + CONFIG_START_CHAT_SERVER;
        public const string CONFIG_START_DATABASE_SERVER = "startDatabaseServer";
        public const string ARG_START_DATABASE_SERVER = "-" + CONFIG_START_DATABASE_SERVER;

        [Header("Server Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager;
        [SerializeField]
        private MapSpawnNetworkManager mapSpawnNetworkManager;
        [SerializeField]
        private MapNetworkManager mapNetworkManager;
        [SerializeField]
        private ChatNetworkManager chatNetworkManager;
        [SerializeField]
        private DatabaseNetworkManager databaseNetworkManager;

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapSpawnNetworkManager MapSpawnNetworkManager { get { return mapSpawnNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public ChatNetworkManager ChatNetworkManager { get { return chatNetworkManager; } }
        public DatabaseNetworkManager DatabaseNetworkManager { get { return databaseNetworkManager; } }
        public bool UseWebSocket { get { return useWebSocket; } }

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
        public bool startChatOnAwake;
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
        private bool startingChatServer;
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

            GameInstance gameInstance = FindObjectOfType<GameInstance>();

            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

            // Active WebSockets
            CentralNetworkManager.useWebSocket = UseWebSocket;
            MapSpawnNetworkManager.useWebSocket = UseWebSocket;
            MapNetworkManager.useWebSocket = UseWebSocket;
            ChatNetworkManager.useWebSocket = UseWebSocket;

            CacheLogGUI.enabled = false;
#if UNITY_STANDALONE && !CLIENT_BUILD
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

                // Central network address
                string centralNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_ADDRESS, out centralNetworkAddress, "localhost") ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_ADDRESS, out centralNetworkAddress, "localhost"))
                {
                    mapSpawnNetworkManager.centralNetworkAddress = centralNetworkAddress;
                    mapNetworkManager.centralNetworkAddress = centralNetworkAddress;
                    chatNetworkManager.centralNetworkAddress = centralNetworkAddress;
                }

                // Central network port
                int centralNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_PORT, out centralNetworkPort, 6000) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_PORT, out centralNetworkPort, 6000))
                {
                    centralNetworkManager.networkPort = centralNetworkPort;
                    mapSpawnNetworkManager.centralNetworkPort = centralNetworkPort;
                    mapNetworkManager.centralNetworkPort = centralNetworkPort;
                    chatNetworkManager.centralNetworkPort = centralNetworkPort;
                }

                // Central max connections
                int centralMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, 1100) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, 1100))
                {
                    centralNetworkManager.maxConnections = centralMaxConnections;
                }

                // Machine network address, will be set to map spawn / map / chat
                string machineNetworkAddress;
                if (ConfigReader.ReadArgs(args, ARG_MACHINE_ADDRESS, out machineNetworkAddress, "localhost") ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MACHINE_ADDRESS, out machineNetworkAddress, "localhost"))
                {
                    mapSpawnNetworkManager.machineAddress = machineNetworkAddress;
                    mapNetworkManager.machineAddress = machineNetworkAddress;
                    chatNetworkManager.machineAddress = machineNetworkAddress;
                }

                // Map spawn network port
                int mapSpawnNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, 6001) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, 6001))
                {
                    mapSpawnNetworkManager.networkPort = mapSpawnNetworkPort;
                }

                // Map spawn max connections
                int mapSpawnMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_MAP_SPAWN_MAX_CONNECTIONS, out mapSpawnMaxConnections, 1100) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_MAP_SPAWN_MAX_CONNECTIONS, out mapSpawnMaxConnections, 1100))
                {
                    mapSpawnNetworkManager.maxConnections = mapSpawnMaxConnections;
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

                // Chat network port
                int chatNetworkPort;
                if (ConfigReader.ReadArgs(args, ARG_CHAT_PORT, out chatNetworkPort, 6003) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CHAT_PORT, out chatNetworkPort, 6003))
                {
                    chatNetworkManager.networkPort = chatNetworkPort;
                }

                // Chat max connections
                int chatMaxConnections;
                if (ConfigReader.ReadArgs(args, ARG_CHAT_MAX_CONNECTIONS, out chatMaxConnections, 1100) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_CHAT_MAX_CONNECTIONS, out chatMaxConnections, 1100))
                {
                    chatNetworkManager.maxConnections = chatMaxConnections;
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
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingDatabaseServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_CENTRAL_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_CENTRAL_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Central";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_CHAT_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_CHAT_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Chat";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingChatServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_MAP_SPAWN_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "MapSpawn";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SERVER) ||
                    (ConfigReader.ReadConfigs(jsonConfig, CONFIG_START_MAP_SERVER, out tempStartServer) && tempStartServer))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Map(" + mapId + ") Instance(" + instanceId + ")";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingMapServer = true;
                }

                if (startLog)
                {
                    CacheLogGUI.logFileName = logFileName;
                    CacheLogGUI.enabled = true;
                }
            }
            else
            {
                gameInstance.SetOnGameDataLoadedCallback(() =>
                {
                    OnGameDataLoaded();
                    gameInstance.OnGameDataLoaded();
                });

                DatabaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);

                if (startDatabaseOnAwake)
                    startingDatabaseServer = true;

                if (startCentralOnAwake)
                    startingCentralServer = true;

                if (startChatOnAwake)
                    startingChatServer = true;

                if (startMapSpawnOnAwake)
                    startingMapSpawnServer = true;

                if (startMapOnAwake)
                {
                    // If run map-server, don't load home scene (home scene load in `Game Instance`)
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
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

            if (startingChatServer)
            {
                // Start chat manager server
                StartChatServer();
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

            if (startingCentralServer ||
                startingMapSpawnServer ||
                startingChatServer ||
                startingMapServer)
            {
                // Start database manager client, it will connect to database manager server
                // To request database functions
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

        public void StartChatServer()
        {
            chatNetworkManager.StartServer();
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
