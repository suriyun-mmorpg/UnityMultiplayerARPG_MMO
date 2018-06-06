using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class MMOClientInstance : MonoBehaviour
    {
        public static MMOClientInstance Singleton { get; protected set; }

        [Header("Client Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager;
        [SerializeField]
        private MapNetworkManager mapNetworkManager;

        [Header("Settings")]
        public MmoNetworkSetting[] networkSettings;

        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;
        }

        private void OnEnable()
        {
            centralNetworkManager.onClientConnected += OnCentralServerConnected;
            centralNetworkManager.onClientDisconnected += OnCentralServerDisconnected;
        }

        private void OnDisable()
        {
            centralNetworkManager.onClientConnected -= OnCentralServerConnected;
            centralNetworkManager.onClientDisconnected -= OnCentralServerDisconnected;
        }

        public void OnCentralServerConnected(NetPeer netPeer)
        {
            if (onClientConnected != null)
                onClientConnected.Invoke(netPeer);
        }

        public void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            if (onClientDisconnected != null)
                onClientDisconnected.Invoke(netPeer);
        }

        #region Client functions
        public void StartCentralClient()
        {
            centralNetworkManager.StartClient();
        }

        public void StartCentralClient(string address, int port)
        {
            centralNetworkManager.networkAddress = address;
            centralNetworkManager.networkPort = port;
            StartCentralClient();
        }

        public void StartMapClient()
        {
            mapNetworkManager.StartClient();
        }

        public void RequestUserLogin(string username, string password, AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserLogin(username, password, callback);
        }

        public void RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserRegister(username, password, callback);
        }
        #endregion
    }
}
