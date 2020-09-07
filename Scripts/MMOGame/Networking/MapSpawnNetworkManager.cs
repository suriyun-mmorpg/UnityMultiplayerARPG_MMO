using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using ConcurrentCollections;

namespace MultiplayerARPG.MMO
{
    public partial class MapSpawnNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        [Header("Central Network Connection")]
        public BaseTransportFactory centralTransportFactory;
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        [Header("Map Spawn Settings")]
        public string exePath = "./Build.exe";
        public bool notSpawnInBatchMode = false;
        public int startPort = 8000;
        public List<BaseMapInfo> spawningMaps;

        [Header("Running In Editor")]
        public bool isOverrideExePath;
        public string overrideExePath = "./Build.exe";
        public bool editorNotSpawnInBatchMode;

        private int spawningPort = -1;
        private int portCounter = -1;
        /// <summary>
        /// Free ports which can use for start map server
        /// </summary>
        private readonly ConcurrentQueue<int> freePorts = new ConcurrentQueue<int>();
        /// <summary>
        /// Actions which will invokes in main thread
        /// </summary>
        private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
        /// <summary>
        /// Map servers processes id
        /// </summary>
        private readonly ConcurrentHashSet<int> processes = new ConcurrentHashSet<int>();
        /// <summary>
        /// List of Map servers that restarting in update loop
        /// </summary>
        private readonly ConcurrentQueue<string> restartingScenes = new ConcurrentQueue<string>();

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
            get { return centralTransportFactory; }
        }

        public CentralAppServerRegister CentralAppServerRegister { get; private set; }

        public string CentralNetworkAddress { get { return centralNetworkAddress; } }
        public int CentralNetworkPort { get { return centralNetworkPort; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppExtra { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapSpawnServer; } }

        protected override void Awake()
        {
            base.Awake();
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
            CentralAppServerRegister = new CentralAppServerRegister(CentralTransportFactory.Build(), this);
            CentralAppServerRegister.onAppServerRegistered = OnAppServerRegistered;
            CentralAppServerRegister.RegisterMessage(MMOMessageTypes.RequestSpawnMap, HandleRequestSpawnMap);
            this.InvokeInstanceDevExtMethods("OnInitCentralAppServerRegister");
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            spawningPort = -1;
            portCounter = -1;
            // Clear free ports
            int tempFreePort;
            while (freePorts.TryDequeue(out tempFreePort))
            {
                // Do nothing
            }
            // Clear main thread actions
            Action tempMainThreadAction;
            while (mainThreadActions.TryDequeue(out tempMainThreadAction))
            {
                // Do nothing
            }
            // Clear processes
            List<int> processIds = new List<int>(processes);
            foreach (int processId in processIds)
            {
                Process.GetProcessById(processId).Kill();
                processes.TryRemove(processId);
            }
            // Clear restarting scenes
            string tempRestartingScenes;
            while (restartingScenes.TryDequeue(out tempRestartingScenes))
            {
                // Do nothing
            }
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

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (IsServer)
            {
                CentralAppServerRegister.Update();
                if (CentralAppServerRegister.IsRegisteredToCentralServer)
                {
                    if (restartingScenes.Count > 0)
                    {
                        string tempRestartingScenes;
                        while (restartingScenes.TryDequeue(out tempRestartingScenes))
                        {
                            SpawnMap(tempRestartingScenes, true);
                        }
                    }
                }
                if (mainThreadActions.Count > 0)
                {
                    Action tempMainThreadAction;
                    while (mainThreadActions.TryDequeue(out tempMainThreadAction))
                    {
                        if (tempMainThreadAction != null)
                            tempMainThreadAction.Invoke();
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
            else if (string.IsNullOrEmpty(message.mapId))
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
                if (spawningMaps == null || spawningMaps.Count == 0)
                {
                    spawningMaps = new List<BaseMapInfo>();
                    spawningMaps.AddRange(GameInstance.MapInfos.Values);
                }
                SpawnMaps(spawningMaps).Forget();
            }
        }

        private async UniTaskVoid SpawnMaps(List<BaseMapInfo> spawningMaps)
        {
            foreach (BaseMapInfo map in spawningMaps)
            {
                SpawnMap(map.Id, true);
                // Add some delay before spawn next map
                await UniTask.Delay(100, true);
            }
        }

        private void FreePort(int port)
        {
            freePorts.Enqueue(port);
        }

        private void SpawnMap(RequestSpawnMapMessage message, bool autoRestart)
        {
            SpawnMap(message.mapId, autoRestart, message);
        }

        private void SpawnMap(string mapId, bool autoRestart, RequestSpawnMapMessage message = null)
        {
            // Port to run map server
            if (freePorts.Count > 0)
            {
                freePorts.TryDequeue(out spawningPort);
            }
            else
            {
                spawningPort = portCounter++;
            }
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
                Logging.Log(LogTag, "Starting process from: " + path);

            // Spawning Process Info
            ProcessStartInfo startProcessInfo = new ProcessStartInfo(path)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = (!NotSpawnInBatchMode ? "-batchmode -nographics " : string.Empty) +
                    $"{MMOServerInstance.ARG_MAP_ID} {mapId} " +
                    (message != null ?
                        $"{MMOServerInstance.ARG_INSTANCE_ID} {message.instanceId} " +
                        $"{MMOServerInstance.ARG_INSTANCE_POSITION_X} {message.instanceWarpPosition.x} " +
                        $"{MMOServerInstance.ARG_INSTANCE_POSITION_Y} {message.instanceWarpPosition.y} " +
                        $"{MMOServerInstance.ARG_INSTANCE_POSITION_Z} {message.instanceWarpPosition.z} " +
                        $"{(message.instanceWarpOverrideRotation ? MMOServerInstance.ARG_INSTANCE_OVERRIDE_ROTATION : string.Empty)} " +
                        $"{MMOServerInstance.ARG_INSTANCE_ROTATION_X} {message.instanceWarpRotation.x} " +
                        $"{MMOServerInstance.ARG_INSTANCE_ROTATION_Y} {message.instanceWarpRotation.y} " +
                        $"{MMOServerInstance.ARG_INSTANCE_ROTATION_Z} {message.instanceWarpRotation.z} "
                        : string.Empty) +
                    $"{MMOServerInstance.ARG_CENTRAL_ADDRESS} {centralNetworkAddress} " +
                    $"{MMOServerInstance.ARG_CENTRAL_PORT} {centralNetworkPort} " +
                    $"{MMOServerInstance.ARG_MACHINE_ADDRESS} {machineAddress} " +
                    $"{MMOServerInstance.ARG_MAP_PORT} {port} " +
                    $"{MMOServerInstance.ARG_START_MAP_SERVER} ",
            };

            if (LogInfo)
                Logging.Log(LogTag, "Starting process with args: " + startProcessInfo.Arguments);

            int processId = 0;
            bool processStarted = false;
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        using (Process process = Process.Start(startProcessInfo))
                        {
                            processId = process.Id;
                            processes.Add(processId);

                            processStarted = true;

                            mainThreadActions.Enqueue(() =>
                            {
                                if (LogInfo)
                                    Logging.Log(LogTag, "Process started. Id: " + processId);
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
                            mainThreadActions.Enqueue(() =>
                            {
                                if (LogFatal)
                                {
                                    Logging.LogError(LogTag, "Tried to start a process at: '" + path + "' but it failed. Make sure that you have set correct the 'exePath' in 'MapSpawnNetworkManager' component");
                                    Logging.LogException(LogTag, e);
                                }

                                // Notify server that it failed to spawn map scene handled the request
                                if (message != null)
                                    ReponseMapSpawn(message.ackId, ResponseSpawnMapMessage.Error.CannotExecute);
                            });
                        }
                    }
                    finally
                    {
                        // Remove the process
                        processes.TryRemove(processId);

                        // Restarting scene
                        if (autoRestart)
                            restartingScenes.Enqueue(mapId);

                        mainThreadActions.Enqueue(() =>
                        {
                            // Release the port number
                            FreePort(port);

                            if (LogInfo)
                                Logging.Log(LogTag, "Process spawn id: " + processId + " killed.");
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
                    restartingScenes.Enqueue(mapId);

                if (LogFatal)
                    Logging.LogException(LogTag, e);
            }
        }

        private void ReponseMapSpawn(uint ackId, ResponseSpawnMapMessage.Error error)
        {
            ResponseSpawnMapMessage responseMessage = new ResponseSpawnMapMessage();
            responseMessage.ackId = ackId;
            responseMessage.responseCode = error == ResponseSpawnMapMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            CentralAppServerRegister.SendResponse(MMOMessageTypes.ResponseSpawnMap, responseMessage);
        }
    }
}
