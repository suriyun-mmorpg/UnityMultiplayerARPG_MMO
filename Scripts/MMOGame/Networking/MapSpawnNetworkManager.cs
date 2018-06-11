using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using System.Diagnostics;
using LiteNetLib;
using System.IO;
using System;
using System.Threading;

namespace Insthync.MMOG
{
    public class MapSpawnNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        [Header("Central Network Connection")]
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        [Header("Map Spawn Settings")]
        public string exePath = "./Build.exe";
        public bool notSpawnInBatchMode = false;
        public int startPort = 8000;

        [Header("Running In Editor")]
        public bool isOverrideExePath;
        public string overrideExePath = "./Build.exe";
        public bool editorNotSpawnInBatchMode;

        private int spawningPort = -1;
        private int portCounter = -1;
        private readonly Queue<int> freePorts = new Queue<int>();
        private readonly object mainThreadLock = new object();
        private readonly List<Action> mainThreadActions = new List<Action>();
        private object processLock = new object();
        private Dictionary<uint, Process> processes = new Dictionary<uint, Process>();

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

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null)
                {
                    cacheCentralAppServerRegister = new CentralAppServerRegister(this);
                    cacheCentralAppServerRegister.RegisterMessage(MessageTypes.RequestSpawnMap, HandleRequestSpawnMap);
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

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
        }

        public override void OnStartServer()
        {
            CentralAppServerRegister.OnStartServer();
            spawningPort = startPort;
            portCounter = startPort;
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            CentralAppServerRegister.OnStopServer();
            base.OnStopServer();
        }

        protected override void Update()
        {
            base.Update();
            if (IsServer && CentralAppServerRegister.IsRegisteredToCentralServer)
            {
                if (mainThreadActions.Count > 0)
                {
                    lock (mainThreadLock)
                    {
                        foreach (var actions in mainThreadActions)
                        {
                            actions.Invoke();
                        }

                        mainThreadActions.Clear();
                    }
                }
            }
            if (IsServer)
                CentralAppServerRegister.PollEvents();
        }

        protected override void OnDestroy()
        {
            foreach (var process in processes.Values)
            {
                process.Kill();
            }
            processes.Clear();
            CentralAppServerRegister.Stop();
            base.OnDestroy();
        }

        private void HandleRequestSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestSpawnMapMessage>();
            var error = ResponseSpawnMapMessage.Error.None;
            if (!CentralAppServerRegister.IsRegisteredToCentralServer)
                error = ResponseSpawnMapMessage.Error.NotReady;
            else if (CentralAppServerRegister.Peer == null || CentralAppServerRegister.Peer != peer)
                error = ResponseSpawnMapMessage.Error.Unauthorized;
            else if (string.IsNullOrEmpty(message.sceneName))
                error = ResponseSpawnMapMessage.Error.EmptySceneName;

            if (error != ResponseSpawnMapMessage.Error.None)
                ReponseMapSpawn(peer, message.ackId, error);
            else
                SpawnMap(peer, message);
        }

        private void FreePort(int port)
        {
            freePorts.Enqueue(port);
        }

        private void SpawnMap(NetPeer peer, RequestSpawnMapMessage message)
        {
            // Port to run map server
            if (freePorts.Count > 0)
                spawningPort = freePorts.Dequeue();
            else
                spawningPort = portCounter++;
            var port = spawningPort;

            // Path to executable
            var path = ExePath;
            if (string.IsNullOrEmpty(path))
            {
                path = File.Exists(Environment.GetCommandLineArgs()[0])
                    ? Environment.GetCommandLineArgs()[0]
                    : Process.GetCurrentProcess().MainModule.FileName;
            }

            // Spawning Process Info
            var startProcessInfo = new ProcessStartInfo(path)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = " " +
                    (!NotSpawnInBatchMode ? "-batchmode -nographics " : "") +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_SCENE_NAME, message.sceneName) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_CENTRAL_ADDRESS, centralNetworkAddress) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_CENTRAL_PORT, centralNetworkPort) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_MACHINE_ADDRESS, machineAddress) +
                    string.Format("{0} {1} ", MMOServerInstance.ARG_MAP_PORT, port) + 
                    " " + MMOServerInstance.ARG_START_MAP_SERVER,
            };

            UnityEngine.Debug.Log("Starting process with args: " + startProcessInfo.Arguments);

            var processStarted = false;
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        UnityEngine.Debug.Log("New thread started");

                        using (var process = Process.Start(startProcessInfo))
                        {
                            UnityEngine.Debug.Log("Process started. Spawn Id: " + message.ackId + ", pid: " + process.Id);
                            processStarted = true;

                            lock (processLock)
                            {
                                // Save the process
                                processes[message.ackId] = process;
                            }

                            // Notify server that we've successfully handled the request
                            ExecuteOnMainThread(() =>
                            {
                                ReponseMapSpawn(peer, message.ackId, ResponseSpawnMapMessage.Error.None);
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
                                ReponseMapSpawn(peer, message.ackId, ResponseSpawnMapMessage.Error.CannotExecute);
                            });
                        }
                        UnityEngine.Debug.LogError("Tried to start a process at: '" + path + "' but it failed. Make sure that you have set correct the 'exePath' in 'MapSpawnNetworkManager' component");
                        UnityEngine.Debug.LogException(e);
                    }
                    finally
                    {
                        lock (processLock)
                        {
                            // Remove the process
                            processes.Remove(message.ackId);
                        }

                        ExecuteOnMainThread(() =>
                        {
                            // Release the port number
                            FreePort(port);

                            UnityEngine.Debug.Log("Process spawn id: " + message.ackId + " killed.");
                        });
                    }

                }).Start();
            }
            catch (Exception e)
            {
                ReponseMapSpawn(peer, message.ackId, ResponseSpawnMapMessage.Error.Unknow);
                UnityEngine.Debug.LogException(e);
            }
        }

        private void ReponseMapSpawn(NetPeer peer, uint ackId, ResponseSpawnMapMessage.Error error)
        {
            var responseMessage = new ResponseSpawnMapMessage();
            responseMessage.ackId = ackId;
            responseMessage.responseCode = error == ResponseSpawnMapMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, peer, MessageTypes.ResponseSpawnMap, responseMessage);
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
