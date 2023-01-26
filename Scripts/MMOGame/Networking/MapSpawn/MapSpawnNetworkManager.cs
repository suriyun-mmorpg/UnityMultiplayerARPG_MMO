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
    [DefaultExecutionOrder(-895)]
    public partial class MapSpawnNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        [Header("Central Network Connection")]
        public string clusterServerAddress = "127.0.0.1";
        public int clusterServerPort = 6010;
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

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public ClusterClient ClusterClient { get; private set; }
#endif
        public string ClusterServerAddress { get { return clusterServerAddress; } }
        public int ClusterServerPort { get { return clusterServerPort; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppExtra { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapSpawnServer; } }

        protected override void Start()
        {
            useWebSocket = false;
            maxConnections = int.MaxValue;
            base.Start();
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            ClusterClient = new ClusterClient(this);
            ClusterClient.onResponseAppServerRegister = OnResponseAppServerRegister;
            ClusterClient.RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleRequestSpawnMap);
#endif
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            spawningPort = -1;
            portCounter = -1;
            // Clear free ports
            while (freePorts.TryDequeue(out _))
            {
                // Do nothing
            }
            // Clear main thread actions
            while (mainThreadActions.TryDequeue(out _))
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
            while (restartingScenes.TryDequeue(out _))
            {
                // Do nothing
            }
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            ClusterClient.OnAppStart();
            spawningPort = startPort;
            portCounter = startPort;
            base.OnStartServer();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void OnStopServer()
        {
            ClusterClient.OnAppStop();
            Clean();
            base.OnStopServer();
        }
#endif

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

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (IsServer)
            {
                ClusterClient.Update();
                if (ClusterClient.IsAppRegistered)
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
#endif

        protected override void OnDestroy()
        {
            Clean();
            base.OnDestroy();
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        internal async UniTaskVoid HandleRequestSpawnMap(
            RequestHandlerData requestHandler,
            RequestSpawnMapMessage request,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result)
        {
            await UniTask.Yield();
            UITextKeys message = UITextKeys.NONE;
            if (!ClusterClient.IsAppRegistered)
                message = UITextKeys.UI_ERROR_APP_NOT_READY;
            else if (string.IsNullOrEmpty(request.mapId))
                message = UITextKeys.UI_ERROR_EMPTY_SCENE_NAME;

            if (message != UITextKeys.NONE)
            {
                result.InvokeError(new ResponseSpawnMapMessage()
                {
                    message = message
                });
            }
            else
            {
                SpawnMap(request, result, false);
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void OnResponseAppServerRegister(AckResponseCode responseCode)
        {
            if (responseCode != AckResponseCode.Success)
                return;
            if (spawningMaps == null || spawningMaps.Count == 0)
            {
                spawningMaps = new List<BaseMapInfo>();
                spawningMaps.AddRange(GameInstance.MapInfos.Values);
            }
            SpawnMaps(spawningMaps).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTaskVoid SpawnMaps(List<BaseMapInfo> spawningMaps)
        {
            foreach (BaseMapInfo map in spawningMaps)
            {
                SpawnMap(map.Id, true);
                // Add some delay before spawn next map
                await UniTask.Delay(100, true);
            }
        }
#endif

        private void FreePort(int port)
        {
            freePorts.Enqueue(port);
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void SpawnMap(
            RequestSpawnMapMessage message,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result,
            bool autoRestart)
        {
            SpawnMap(message.mapId, autoRestart, message, result);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void SpawnMap(
            string mapId, bool autoRestart,
            RequestSpawnMapMessage? request = null,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result = null)
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
                    (request.HasValue ?
                        $"{MMOServerInstance.ARG_INSTANCE_ID} {request.Value.instanceId} " +
                        $"{MMOServerInstance.ARG_INSTANCE_POSITION_X} {request.Value.instanceWarpPosition.x} " +
                        $"{MMOServerInstance.ARG_INSTANCE_POSITION_Y} {request.Value.instanceWarpPosition.y} " +
                        $"{MMOServerInstance.ARG_INSTANCE_POSITION_Z} {request.Value.instanceWarpPosition.z} " +
                        $"{(request.Value.instanceWarpOverrideRotation ? MMOServerInstance.ARG_INSTANCE_OVERRIDE_ROTATION : string.Empty)} " +
                        $"{MMOServerInstance.ARG_INSTANCE_ROTATION_X} {request.Value.instanceWarpRotation.x} " +
                        $"{MMOServerInstance.ARG_INSTANCE_ROTATION_Y} {request.Value.instanceWarpRotation.y} " +
                        $"{MMOServerInstance.ARG_INSTANCE_ROTATION_Z} {request.Value.instanceWarpRotation.z} "
                        : string.Empty) +
                    $"{MMOServerInstance.ARG_CENTRAL_ADDRESS} {clusterServerAddress} " +
                    $"{MMOServerInstance.ARG_CENTRAL_PORT} {clusterServerPort} " +
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
                                if (request.HasValue && result != null)
                                {
                                    result.InvokeSuccess(new ResponseSpawnMapMessage()
                                    {
                                        message = UITextKeys.NONE,
                                        instanceId = request.Value.instanceId,
                                        requestId = request.Value.requestId,
                                    });
                                }
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
                                if (request.HasValue && result != null)
                                {
                                    result.InvokeError(new ResponseSpawnMapMessage()
                                    {
                                        message = UITextKeys.UI_ERROR_CANNOT_EXCUTE_MAP_SERVER,
                                        instanceId = request.Value.instanceId,
                                        requestId = request.Value.requestId,
                                    });
                                }
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
                if (request.HasValue && result != null)
                {
                    result.InvokeError(new ResponseSpawnMapMessage()
                    {
                        message = UITextKeys.UI_ERROR_UNKNOW,
                        instanceId = request.Value.instanceId,
                        requestId = request.Value.requestId,
                    });
                }

                // Restarting scene
                if (autoRestart)
                    restartingScenes.Enqueue(mapId);

                if (LogFatal)
                    Logging.LogException(LogTag, e);
            }
        }
#endif
    }
}
