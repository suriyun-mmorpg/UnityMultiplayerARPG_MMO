using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using System.Diagnostics;
using LiteNetLib;
using System.IO;
using System;
using System.Threading;

namespace MultiplayerARPG.MMO
{
    public partial class MapSpawnNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        [Header("Central Network Connection")]
        public BaseTransportFactory centralTransportFactory;
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        [Header("Map Spawn Settings")]
        public string exePath = "./Build.exe";
        public bool notSpawnInBatchMode = false;
        public int startPort = 8000;
        public List<UnityScene> spawningScenes;

        [Header("Running In Editor")]
        public bool isOverrideExePath;
        public string overrideExePath = "./Build.exe";
        public bool editorNotSpawnInBatchMode;

        private int spawningPort = -1;
        private int portCounter = -1;
        private readonly Queue<int> freePorts = new Queue<int>();
        private readonly object mainThreadLock = new object();
        private readonly List<Action> mainThreadActions = new List<Action>();
        private readonly object processLock = new object();
        private uint processIdCounter = 0;
        /// <summary>
        /// Dictionary of Map servers processes
        /// </summary>
        private readonly Dictionary<uint, Process> processes = new Dictionary<uint, Process>();
        /// <summary>
        /// List of Map servers that restarting in update loop
        /// </summary>
        private readonly List<string> restartingScenes = new List<string>();

        public string ExePath
        {
            get
            {
                if (Application.isEditor && isOverrideExePath)
                    return overrideExePath;
                else
                    return exePath;
            }
        }

        public bool NotSpawnInBatchMode
        {
            get
            {
                if (Application.isEditor)
                    return editorNotSpawnInBatchMode;
                else
                    return notSpawnInBatchMode;
            }
        }
        
        public BaseTransportFactory CentralTransportFactory
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // Force to use websocket transport if it's running as webgl
                if (centralTransportFactory == null || !centralTransportFactory.CanUseWithWebGL)
                    centralTransportFactory = gameObject.AddComponent<WebSocketTransportFactory>();
#else
                if (useWebSocket)
                {
                    if (centralTransportFactory == null || !centralTransportFactory.CanUseWithWebGL)
                        centralTransportFactory = gameObject.AddComponent<WebSocketTransportFactory>();
                }
                else
                {
                    if (centralTransportFactory == null)
                        centralTransportFactory = gameObject.AddComponent<LiteNetLibTransportFactory>();
                }
#endif
                return centralTransportFactory;
            }
        }

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null)
                {
                    cacheCentralAppServerRegister = new CentralAppServerRegister(CentralTransportFactory.Build(), this);
                    cacheCentralAppServerRegister.onAppServerRegistered = OnAppServerRegistered;
                    cacheCentralAppServerRegister.RegisterMessage(MMOMessageTypes.RequestSpawnMap, HandleRequestSpawnMap);
                }
                return cacheCentralAppServerRegister;
            }
        }

        public string CentralNetworkAddress { get { return centralNetworkAddress; } }
        public int CentralNetworkPort { get { return centralNetworkPort; } }
        public string CentralConnectKey { get { return centralConnectKey; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppConnectKey { get { return connectKey; } }
        public string AppExtra { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapSpawnServer; } }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            spawningPort = -1;
            portCounter = -1;
            freePorts.Clear();
            mainThreadActions.Clear();
            processIdCounter = 0;
            foreach (Process process in processes.Values)
            {
                process.Kill();
            }
            processes.Clear();
            restartingScenes.Clear();
        }

        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            CentralAppServerRegister.OnStartServer();
            spawningPort = startPort;
            portCounter = startPort;
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            CentralAppServerRegister.OnStopServer();
            Clean();
            base.OnStopServer();
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        protected override void Update()
        {
            base.Update();
            if (IsServer)
            {
                CentralAppServerRegister.Update();
                if (CentralAppServerRegister.IsRegisteredToCentralServer)
                {
                    if (mainThreadActions.Count > 0)
                    {
                        lock (mainThreadLock)
                        {
                            foreach (Action actions in mainThreadActions)
                            {
                                actions.Invoke();
                            }

                            mainThreadActions.Clear();
                        }
                    }
                    if (restartingScenes.Count > 0)
                    {
                        foreach (string scene in restartingScenes)
                        {
                            SpawnMap(scene, true);
                        }
                        restartingScenes.Clear();
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            Clean();
            base.OnDestroy();
        }

        private void HandleRequestSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            RequestSpawnMapMessage message = messageHandler.ReadMessage<RequestSpawnMapMessage>();
            ResponseSpawnMapMessage.Error error = ResponseSpawnMapMessage.Error.None;
            if (!CentralAppServerRegister.IsRegisteredToCentralServer)
                error = ResponseSpawnMapMessage.Error.NotReady;
            else if (string.IsNullOrEmpty(message.sceneName))
                error = ResponseSpawnMapMessage.Error.EmptySceneName;

            if (error != ResponseSpawnMapMessage.Error.None)
                ReponseMapSpawn(message.ackId, error);
            else
                SpawnMap(message, false);
        }

        private void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message)
        {
            if (responseCode == AckResponseCode.Success)
            {
                if (spawningScenes == null || spawningScenes.Count == 0)
                {
                    spawningScenes = new List<UnityScene>();
                    List<string> sceneNames = GameInstance.Singleton.GetGameScenes();
                    foreach (string sceneName in sceneNames)
                    {
                        spawningScenes.Add(new UnityScene()
                        {
                            SceneName = sceneName
                        });
                    }
                }
                foreach (UnityScene scene in spawningScenes)
                {
                    SpawnMap(scene, true);
                }
            }
        }

        private void FreePort(int port)
        {
            freePorts.Enqueue(port);
        }

        private void SpawnMap(RequestSpawnMapMessage message, bool autoRestart)
        {
            SpawnMap(message.sceneName, autoRestart, message);
        }

        private void SpawnMap(string sceneName, bool autoRestart, RequestSpawnMapMessage message = null)
        {
            // Port to run map server
            if (freePorts.Count > 0)
                spawningPort = freePorts.Dequeue();
            else
                spawningPort = portCounter++;
            int port = spawningPort;

            // Path to executable
            string path = ExePath;
            if (string.IsNullOrEmpty(path))
            {
                path = File.Exists(Environment.GetCommandLineArgs()[0])
                    ? Environment.GetCommandLineArgs()[0]
                    : Process.GetCurrentProcess().MainModule.FileName;
            }

            if (LogInfo)
                UnityEngine.Debug.Log("Starting process from: " + path);

            // Spawning Process Info
            ProcessStartInfo startProcessInfo = new ProcessStartInfo(path)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = " " +
                    (!NotSpawnInBatchMode ? "-batchmode -nographics " : "") +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_SCENE_NAME, sceneName) +
                    (message != null ? string.Format("{0} {1} ", MMOServerInstance.ARG_INSTANCE_ID, message.instanceId) : "") +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_CENTRAL_ADDRESS, centralNetworkAddress) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_CENTRAL_PORT, centralNetworkPort) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_MACHINE_ADDRESS, machineAddress) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_MAP_PORT, port) +
                    " " + MMOServerInstance.ARG_START_MAP_SERVER,
            };

            if (LogInfo)
                UnityEngine.Debug.Log("Starting process with args: " + startProcessInfo.Arguments);

            uint processId = ++processIdCounter;
            bool processStarted = false;
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        using (Process process = Process.Start(startProcessInfo))
                        {
                            lock (processLock)
                            {
                                // Save the process
                                processes[processId] = process;
                            }

                            processStarted = true;

                            ExecuteOnMainThread(() =>
                            {
                                if (LogInfo)
                                    UnityEngine.Debug.Log("Process started. Spawn Id: " + processId + ", pid: " + process.Id);
                                // Notify server that it's successfully handled the request
                                if (message != null)
                                    ReponseMapSpawn(message.ackId, ResponseSpawnMapMessage.Error.None);
                            });
                            process.WaitForExit();
                        }
                    }
                    catch (Exception e)
                    {
                        if (!processStarted)
                        {
                            ExecuteOnMainThread(() =>
                            {
                                if (LogFatal)
                                {
                                    UnityEngine.Debug.LogError("Tried to start a process at: '" + path + "' but it failed. Make sure that you have set correct the 'exePath' in 'MapSpawnNetworkManager' component");
                                    UnityEngine.Debug.LogException(e);
                                }

                                // Notify server that it failed to spawn map scene handled the request
                                if (message != null)
                                    ReponseMapSpawn(message.ackId, ResponseSpawnMapMessage.Error.CannotExecute);
                            });
                        }
                    }
                    finally
                    {
                        lock (processLock)
                        {
                            // Remove the process
                            processes.Remove(processId);

                            // Restarting scene
                            if (autoRestart)
                                restartingScenes.Add(sceneName);
                        }

                        ExecuteOnMainThread(() =>
                        {
                            // Release the port number
                            FreePort(port);

                            if (LogInfo)
                                UnityEngine.Debug.Log("Process spawn id: " + processId + " killed.");
                        });
                    }

                }).Start();
            }
            catch (Exception e)
            {
                if (message != null)
                    ReponseMapSpawn(message.ackId, ResponseSpawnMapMessage.Error.Unknow);

                // Restarting scene
                if (autoRestart)
                    restartingScenes.Add(sceneName);

                if (LogFatal)
                    UnityEngine.Debug.LogException(e);
            }
        }

        private void ReponseMapSpawn(uint ackId, ResponseSpawnMapMessage.Error error)
        {
            ResponseSpawnMapMessage responseMessage = new ResponseSpawnMapMessage();
            responseMessage.ackId = ackId;
            responseMessage.responseCode = error == ResponseSpawnMapMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ClientSendPacket(SendOptions.ReliableOrdered, MMOMessageTypes.ResponseSpawnMap, responseMessage);
        }

        private void ExecuteOnMainThread(Action action)
        {
            lock (mainThreadLock)
            {
                mainThreadActions.Add(action);
            }
        }
    }
}
