using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public class MMOServerInstance : MonoBehaviour
    {
        public static MMOServerInstance Singleton { get; protected set; }

        public const string ARG_START_CENTRAL_SERVER = "-startCentralServer";
        public const string ARG_START_LOGIN_SERVER = "-startLoginServer";
        public const string ARG_START_CHAT_SERVER = "-startChatServer";
        public const string ARG_START_MAP_SPAWN_SERVER = "-startMapSpawnServer";
        public const string ARG_START_MAP_SERVER = "-startMapServer";
        public const string ARG_CENTRAL_ADDRESS = "-centralAddress";
        public const string ARG_CENTRAL_PORT = "-centralPort";
        public const string ARG_LOGIN_ADDRESS = "-loginAddress";
        public const string ARG_LOGIN_PORT = "-loginPort";
        public const string ARG_CHAT_ADDRESS = "-chatAddress";
        public const string ARG_CHAT_PORT = "-chatPort";
        public const string ARG_MAP_SPAWN_ADDRESS = "-mapSpawnAddress";
        public const string ARG_MAP_SPAWN_PORT = "-mapSpawnPort";
        public const string ARG_MAP_SPAWN_EXE_PATH = "-mapSpawnExePath";
        public const string ARG_MAP_SPAWN_IN_BATCH_MODE = "-mapSpawnInBatchMode";
        public const string ARG_MAP_ADDRESS = "-mapAddress";
        public const string ARG_MAP_PORT = "-mapPort";
        public const string ARG_MAP_SCENE_NAME = "-mapSceneName";

        #region Server components
        [Header("Server Components")]
        public CentralNetworkManager centralServerNetworkManager;
        public LoginNetworkManager loginServerNetworkManager;
        public ChatNetworkManager chatServerNetworkManager;
        public MapSpawnNetworkManager mapSpawnServerNetworkManager;
        public MapNetworkManager mapServerNetworkManager;
        #endregion

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
                loginServerNetworkManager.centralServerAddress = address;
                chatServerNetworkManager.centralServerAddress = address;
                mapSpawnServerNetworkManager.centralServerAddress = address;
                mapServerNetworkManager.centralServerAddress = address;
            }

            if (IsArgsProvided(args, ARG_CENTRAL_PORT))
            {
                var port = ReadArgsInt(args, ARG_CENTRAL_PORT, 6000);
                centralServerNetworkManager.networkPort = port;
                loginServerNetworkManager.centralServerPort = port;
                chatServerNetworkManager.centralServerPort = port;
                mapSpawnServerNetworkManager.centralServerPort = port;
                mapServerNetworkManager.centralServerPort = port;
            }

            if (IsArgsProvided(args, ARG_LOGIN_ADDRESS))
            {
                var address = ReadArgs(args, ARG_LOGIN_ADDRESS, "localhost");
                loginServerNetworkManager.machineAddress = address;
            }

            if (IsArgsProvided(args, ARG_LOGIN_PORT))
            {
                var port = ReadArgsInt(args, ARG_LOGIN_PORT, 6001);
                loginServerNetworkManager.networkPort = port;
            }

            if (IsArgsProvided(args, ARG_CHAT_ADDRESS))
            {
                var address = ReadArgs(args, ARG_CHAT_ADDRESS, "localhost");
                chatServerNetworkManager.machineAddress = address;
            }

            if (IsArgsProvided(args, ARG_CHAT_PORT))
            {
                var port = ReadArgsInt(args, ARG_CHAT_PORT, 6002);
                chatServerNetworkManager.networkPort = port;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_ADDRESS))
            {
                var address = ReadArgs(args, ARG_MAP_SPAWN_ADDRESS, "localhost");
                mapSpawnServerNetworkManager.machineAddress = address;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_PORT))
            {
                var port = ReadArgsInt(args, ARG_MAP_SPAWN_PORT, 6003);
                mapSpawnServerNetworkManager.networkPort = port;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_EXE_PATH))
            {
                var exePath = ReadArgs(args, ARG_MAP_SPAWN_EXE_PATH, "./Build.exe");
                mapSpawnServerNetworkManager.exePath = exePath;
            }

            if (IsArgsProvided(args, ARG_MAP_SPAWN_IN_BATCH_MODE))
            {
                var spawnInBatchMode = ReadArgsInt(args, ARG_MAP_SPAWN_IN_BATCH_MODE, 1) > 0;
                mapSpawnServerNetworkManager.spawnInBatchMode = spawnInBatchMode;
            }

            if (IsArgsProvided(args, ARG_MAP_ADDRESS))
            {
                var address = ReadArgs(args, ARG_MAP_ADDRESS, "localhost");
                mapServerNetworkManager.machineAddress = address;
            }

            if (IsArgsProvided(args, ARG_MAP_PORT))
            {
                var port = ReadArgsInt(args, ARG_MAP_PORT, 6004);
                mapServerNetworkManager.networkPort = port;
            }

            if (IsArgsProvided(args, ARG_MAP_SCENE_NAME))
            {
                var sceneName = ReadArgs(args, ARG_MAP_SCENE_NAME);
                mapServerNetworkManager.Assets.onlineScene.SceneName = sceneName;
            }

            if (IsArgsProvided(args, ARG_START_CENTRAL_SERVER))
                StartCentralServer();

            if (IsArgsProvided(args, ARG_START_LOGIN_SERVER))
                StartLoginServer();

            if (IsArgsProvided(args, ARG_START_CHAT_SERVER))
                StartChatServer();

            if (IsArgsProvided(args, ARG_START_MAP_SPAWN_SERVER))
                StartMapSpawnServer();

            if (IsArgsProvided(args, ARG_START_MAP_SERVER))
                StartMapServer();
        }

        #region Server functions
        public void StartCentralServer()
        {
            centralServerNetworkManager.StartServer();
        }

        public void StartLoginServer()
        {
            loginServerNetworkManager.StartServer();
        }

        public void StartChatServer()
        {
            chatServerNetworkManager.StartServer();
        }

        public void StartMapSpawnServer()
        {
            mapSpawnServerNetworkManager.StartServer();
        }

        public void StartMapServer()
        {
            mapServerNetworkManager.StartServer();
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
