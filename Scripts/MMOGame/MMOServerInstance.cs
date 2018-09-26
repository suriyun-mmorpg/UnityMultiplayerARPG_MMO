using System;
using System.Collections.Generic;
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
        public const string CONFIG_SCENE_NAME = "sceneName";
        public const string ARG_SCENE_NAME = "-" + CONFIG_SCENE_NAME;
        // Chat server
        public const string CONFIG_CHAT_PORT = "chatPort";
        public const string ARG_CHAT_PORT = "-" + CONFIG_CHAT_PORT;
        public const string CONFIG_CHAT_MAX_CONNECTIONS = "chatMaxConnections";
        public const string ARG_CHAT_MAX_CONNECTIONS = "-" + CONFIG_CHAT_MAX_CONNECTIONS;
        // Start servers
        public const string ARG_START_CENTRAL_SERVER = "-startCentralServer";
        public const string ARG_START_MAP_SPAWN_SERVER = "-startMapSpawnServer";
        public const string ARG_START_MAP_SERVER = "-startMapServer";
        public const string ARG_START_CHAT_SERVER = "-startChatServer";

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
        private BaseDatabase database;

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapSpawnNetworkManager MapSpawnNetworkManager { get { return mapSpawnNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public ChatNetworkManager ChatNetworkManager { get { return chatNetworkManager; } }
        public BaseDatabase Database { get { return database; } }
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

        private bool startingCentralServer;
        private bool startingMapSpawnServer;
        private bool startingMapServer;
        private bool startingChatServer;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            if (database != null)
                database.Initialize();

            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
            CacheLogGUI.enabled = false;
            if (!Application.isEditor)
            {
                var gameInstance = FindObjectOfType<GameInstance>();

                // Json file read
                var configFilePath = "./config/serverConfig.json";
                var jsonConfig = new Dictionary<string, object>();
                Debug.Log("[MMOServerInstance] Reading config file from " + configFilePath);
                if (File.Exists(configFilePath))
                {
                    Debug.Log("[MMOServerInstance] Found config file");
                    string dataAsJson = File.ReadAllText(configFilePath);
                    jsonConfig = Json.Deserialize(dataAsJson) as Dictionary<string, object>;
                }

                // Prepare data
                var args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

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
                List<string> spawnMaps;
                if (ConfigReader.ReadArgs(args, ARG_SPAWN_MAPS, out spawnMaps, new List<string>()) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SPAWN_MAPS, out spawnMaps, new List<string>()))
                {
                    mapSpawnNetworkManager.spawningScenes = new List<UnityScene>();
                    foreach (var spawnMap in spawnMaps)
                    {
                        mapSpawnNetworkManager.spawningScenes.Add(new UnityScene()
                        {
                            SceneName = spawnMap
                        });
                    }
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
                string mapSceneName;
                if (ConfigReader.ReadArgs(args, ARG_SCENE_NAME, out mapSceneName) ||
                    ConfigReader.ReadConfigs(jsonConfig, CONFIG_SCENE_NAME, out mapSceneName))
                {
                    mapNetworkManager.Assets.onlineScene.SceneName = mapSceneName;
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

                var logFileName = "Log";
                var startLog = false;

                if (ConfigReader.IsArgsProvided(args, ARG_START_CENTRAL_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Central";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "MapSpawn";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_MAP_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Map(" + mapNetworkManager.Assets.onlineScene.SceneName + ")";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingMapServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ARG_START_CHAT_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Chat";
                    startLog = true;
                    gameInstance.SetOnGameDataLoadedCallback(OnGameDataLoaded);
                    startingChatServer = true;
                }

                if (startLog)
                {
                    CacheLogGUI.logFileName = logFileName;
                    CacheLogGUI.enabled = true;
                }
            }
            else
            {
                if (startCentralOnAwake)
                    startingCentralServer = true;

                if (startMapSpawnOnAwake)
                    startingMapSpawnServer = true;

                if (startChatOnAwake)
                    startingChatServer = true;
            }
        }

        private void OnGameDataLoaded()
        {
            if (startingCentralServer)
                StartCentralServer();

            if (startingMapSpawnServer)
                StartMapSpawnServer();

            if (startingMapServer)
                StartMapServer();

            if (startingChatServer)
                StartChatServer();
        }

        private void OnDestroy()
        {
            if (database != null)
                database.Destroy();
        }

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
        #endregion
    }
}
