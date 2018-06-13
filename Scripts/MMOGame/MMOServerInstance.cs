using System;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    [RequireComponent(typeof(LogGUI))]
    public class MMOServerInstance : MonoBehaviour
    {
        public static MMOServerInstance Singleton { get; protected set; }

        public const string ARG_CENTRAL_ADDRESS = "-centralAddress";
        public const string ARG_CENTRAL_PORT = "-centralPort";
        public const string ARG_MACHINE_ADDRESS = "-machineAddress";
        // Map spawn server
        public const string ARG_MAP_SPAWN_PORT = "-mapSpawnPort";
        public const string ARG_MAP_SPAWN_MAX_CONNECTIONS = "-mapSpawnMaxConnections";
        public const string ARG_SPAWN_EXE_PATH = "-spawnExePath";
        public const string ARG_NOT_SPAWN_IN_BATCH_MODE = "-notSpawnInBatchMode";
        // Map server
        public const string ARG_MAP_PORT = "-mapPort";
        public const string ARG_MAP_MAX_CONNECTIONS = "-mapMaxConnections";
        public const string ARG_SCENE_NAME = "-sceneName";
        // Chat server
        public const string ARG_CHAT_PORT = "-chatPort";
        public const string ARG_CHAT_MAX_CONNECTIONS = "-chatMaxConnections";
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
        private readonly List<string> scenes = new List<string>();

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            CacheLogGUI.enabled = false;
            if (!Application.isEditor)
            {
                var gameInstance = FindObjectOfType<GameInstance>();

                // Prepare data
                var args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

                if (IsArgsProvided(args, ARG_CENTRAL_ADDRESS))
                {
                    var address = ReadArgs(args, ARG_CENTRAL_ADDRESS, "localhost");
                    mapSpawnNetworkManager.centralNetworkAddress = address;
                    mapNetworkManager.centralNetworkAddress = address;
                }

                if (IsArgsProvided(args, ARG_CENTRAL_PORT))
                {
                    var port = ReadArgsInt(args, ARG_CENTRAL_PORT, 6000);
                    centralNetworkManager.networkPort = port;
                    mapSpawnNetworkManager.centralNetworkPort = port;
                    mapNetworkManager.centralNetworkPort = port;
                }

                if (IsArgsProvided(args, ARG_MACHINE_ADDRESS))
                {
                    var address = ReadArgs(args, ARG_MACHINE_ADDRESS, "127.0.0.1");
                    mapSpawnNetworkManager.machineAddress = address;
                }

                if (IsArgsProvided(args, ARG_MAP_SPAWN_PORT))
                {
                    var port = ReadArgsInt(args, ARG_MAP_SPAWN_PORT, 6001);
                    mapSpawnNetworkManager.networkPort = port;
                }

                if (IsArgsProvided(args, ARG_MAP_SPAWN_MAX_CONNECTIONS))
                {
                    var maxConnections = ReadArgsInt(args, ARG_MAP_SPAWN_MAX_CONNECTIONS, 1100);
                    mapSpawnNetworkManager.maxConnections = maxConnections;
                }

                if (IsArgsProvided(args, ARG_SPAWN_EXE_PATH))
                {
                    var exePath = ReadArgs(args, ARG_SPAWN_EXE_PATH, "./Build.exe");
                    mapSpawnNetworkManager.exePath = exePath;
                }

                if (IsArgsProvided(args, ARG_NOT_SPAWN_IN_BATCH_MODE))
                    mapSpawnNetworkManager.notSpawnInBatchMode = true;

                if (IsArgsProvided(args, ARG_MACHINE_ADDRESS))
                {
                    var address = ReadArgs(args, ARG_MACHINE_ADDRESS, "127.0.0.1");
                    mapNetworkManager.machineAddress = address;
                }

                if (IsArgsProvided(args, ARG_MAP_PORT))
                {
                    var port = ReadArgsInt(args, ARG_MAP_PORT, 6002);
                    mapNetworkManager.networkPort = port;
                }

                if (IsArgsProvided(args, ARG_MAP_MAX_CONNECTIONS))
                {
                    var maxConnections = ReadArgsInt(args, ARG_MAP_MAX_CONNECTIONS, 1100);
                    mapNetworkManager.maxConnections = maxConnections;
                }

                if (IsArgsProvided(args, ARG_SCENE_NAME))
                {
                    var sceneName = ReadArgs(args, ARG_SCENE_NAME);
                    mapNetworkManager.Assets.onlineScene.SceneName = sceneName;
                }

                if (IsArgsProvided(args, ARG_CHAT_PORT))
                {
                    var port = ReadArgsInt(args, ARG_CHAT_PORT, 6003);
                    chatNetworkManager.networkPort = port;
                }

                if (IsArgsProvided(args, ARG_CHAT_MAX_CONNECTIONS))
                {
                    var maxConnections = ReadArgsInt(args, ARG_CHAT_MAX_CONNECTIONS, 1100);
                    chatNetworkManager.maxConnections = maxConnections;
                }

                var logFileName = "Log";
                var startLog = false;

                if (IsArgsProvided(args, ARG_START_CENTRAL_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Central";
                    startLog = true;
                    gameInstance.doNotLoadHomeSceneOnStart = true;
                    startingCentralServer = true;
                }

                if (IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "MapSpawn";
                    startLog = true;
                    gameInstance.doNotLoadHomeSceneOnStart = true;
                    startingMapSpawnServer = true;
                }

                if (IsArgsProvided(args, ARG_START_MAP_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Map(" + mapNetworkManager.Assets.onlineScene.SceneName + ")";
                    startLog = true;
                    gameInstance.doNotLoadHomeSceneOnStart = true;
                    startingMapServer = true;
                }

                if (IsArgsProvided(args, ARG_START_CHAT_SERVER))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                        logFileName += "_";
                    logFileName += "Chat";
                    startLog = true;
                    gameInstance.doNotLoadHomeSceneOnStart = true;
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

        private void Start()
        {
            scenes.Clear();
            if (GameInstance.Singleton.startScene != null &&
                !string.IsNullOrEmpty(GameInstance.Singleton.startScene.SceneName))
                scenes.Add(GameInstance.Singleton.startScene.SceneName);

            foreach (var scene in GameInstance.Singleton.otherScenes)
            {
                if (scene != null &&
                    !string.IsNullOrEmpty(scene.SceneName) &&
                    !scenes.Contains(scene.SceneName))
                    scenes.Add(scene.SceneName);
            }

            if (startingCentralServer)
                StartCentralServer();

            if (startingMapSpawnServer)
                StartMapSpawnServer();

            if (startingMapServer)
                StartMapServer();

            if (startingChatServer)
                StartChatServer();
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

        public List<string> GetScenes()
        {
            return scenes;
        }
        #endregion
        
        private string ReadArgs(string[] args, string argName, string defaultValue = null)
        {
            if (args == null)
                return defaultValue;

            var argsList = new List<string>(args);
            if (!argsList.Contains(argName))
                return defaultValue;

            var index = argsList.FindIndex(0, a => a.Equals(argName));
            return args[index + 1];
        }

        private int ReadArgsInt(string[] args, string argName, int defaultValue = -1)
        {
            var number = ReadArgs(args, argName, defaultValue.ToString());
            var result = defaultValue;
            if (int.TryParse(number, out result))
                return result;
            return defaultValue;
        }

        private bool IsArgsProvided(string[] args, string argName)
        {
            if (args == null)
                return false;

            var argsList = new List<string>(args);
            return argsList.Contains(argName);
        }
    }
}
