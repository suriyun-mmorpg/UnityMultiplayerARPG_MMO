using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class MapSpawnNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        [Header("Map Spawn Settings")]
        public string exePath = "./Build.exe";
        public bool spawnInBatchMode = false;

        [Header("Running In Editor")]
        public bool isOverrideExePath;
        public string overrideExePath = "./Build.exe";

        private CentralAppServerConnector cacheCentralAppServerConnector;
        public CentralAppServerConnector CentralAppServerConnector
        {
            get
            {
                if (cacheCentralAppServerConnector == null)
                    cacheCentralAppServerConnector = gameObject.AddComponent<CentralAppServerConnector>();
                return cacheCentralAppServerConnector;
            }
        }
        
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            CentralAppServerConnector.OnStartServer(CentralServerPeerType.MapSpawnServer, networkPort, connectKey, string.Empty);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            CentralAppServerConnector.OnStopServer();
        }
    }
}
