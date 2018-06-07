using System;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class MMOServerInstance : MonoBehaviour
    {
        public static MMOServerInstance Singleton { get; protected set; }

        public const string ARG_START_CENTRAL_SERVER = "-startCentralServer";
        public const string ARG_START_MAP_SPAWN_SERVER = "-startMapSpawnServer";
        public const string ARG_START_MAP_SERVER = "-startMapServer";
        public const string ARG_CENTRAL_ADDRESS = "-centralAddress";
        public const string ARG_CENTRAL_PORT = "-centralPort";
        public const string ARG_MAP_SPAWN_ADDRESS = "-mapSpawnAddress";
        public const string ARG_MAP_SPAWN_PORT = "-mapSpawnPort";
        public const string ARG_MAP_SPAWN_EXE_PATH = "-mapSpawnExePath";
        public const string ARG_MAP_SPAWN_IN_BATCH_MODE = "-mapSpawnInBatchMode";
        public const string ARG_MAP_ADDRESS = "-mapAddress";
        public const string ARG_MAP_PORT = "-mapPort";
        public const string ARG_MAP_SCENE_NAME = "-mapSceneName";
        
        [Header("Server Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager;
        [SerializeField]
        private MapSpawnNetworkManager mapSpawnNetworkManager;
        [SerializeField]
        private MapNetworkManager mapNetworkManager;
        [SerializeField]
        private BaseDatabase database;

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapSpawnNetworkManager MapSpawnNetworkManager { get { return mapSpawnNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public BaseDatabase Database { get { return database; } }

        [Header("Gameplay Configs")]
        public LiteNetLibScene[] scenes;

        [Header("Running In Editor")]
        public bool startCentralOnAwake;
        public bool startMapSpawnOnAwake;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            var args = Environment.GetCommandLineArgs();
            
            // Android fix
            if (args == null)
                args = new string[0];

            if (IsArgsProvided(args, ARG_CENTRAL_ADDRESS))
            {
                var address = ReadArgs(args, ARG_CENTRAL_ADDRESS, "localhost");
                mapSpawnNetworkManager.CentralAppServerConnector.centralServerAddress = address;
                mapNetworkManager.CentralAppServerConnector.centralServerAddress = address;
            }

            if (IsArgsProvided(args, ARG_CENTRAL_PORT))
            {
                var port = ReadArgsInt(args, ARG_CENTRAL_PORT, 6000);
                centralNetworkManager.networkPort = port;
                mapSpawnNetworkManager.CentralAppServerConnector.centralServerPort = port;
                mapNetworkManager.CentralAppServerConnector.centralServerPort = port;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_ADDRESS))
            {
                var address = ReadArgs(args, ARG_MAP_SPAWN_ADDRESS, "localhost");
                mapSpawnNetworkManager.CentralAppServerConnector.machineAddress = address;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_PORT))
            {
                var port = ReadArgsInt(args, ARG_MAP_SPAWN_PORT, 6003);
                mapSpawnNetworkManager.networkPort = port;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_EXE_PATH))
            {
                var exePath = ReadArgs(args, ARG_MAP_SPAWN_EXE_PATH, "./Build.exe");
                mapSpawnNetworkManager.exePath = exePath;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_IN_BATCH_MODE))
            {
                var spawnInBatchMode = ReadArgsInt(args, ARG_MAP_SPAWN_IN_BATCH_MODE, 1) > 0;
                mapSpawnNetworkManager.spawnInBatchMode = spawnInBatchMode;
            }

            if (IsArgsProvided(args, ARG_MAP_ADDRESS))
            {
                var address = ReadArgs(args, ARG_MAP_ADDRESS, "localhost");
                mapNetworkManager.CentralAppServerConnector.machineAddress = address;
            }

            if (IsArgsProvided(args, ARG_MAP_PORT))
            {
                var port = ReadArgsInt(args, ARG_MAP_PORT, 6004);
                mapNetworkManager.networkPort = port;
            }

            if (IsArgsProvided(args, ARG_MAP_SCENE_NAME))
            {
                var sceneName = ReadArgs(args, ARG_MAP_SCENE_NAME);
                mapNetworkManager.Assets.onlineScene.SceneName = sceneName;
            }

            if (IsArgsProvided(args, ARG_START_CENTRAL_SERVER) || (Application.isEditor && startCentralOnAwake))
                StartCentralServer();

            if (IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER) || (Application.isEditor && startMapSpawnOnAwake))
                StartMapSpawnServer();

            if (IsArgsProvided(args, ARG_START_MAP_SERVER))
                StartMapServer();
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
