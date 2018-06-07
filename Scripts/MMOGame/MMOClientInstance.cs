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

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }

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
                onClientDisconnected.Invoke(netPeer, disconnectInfo);
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

        public void StartMapClient(string sceneName, string address, int port, string connectKey)
        {
            mapNetworkManager.Assets.onlineScene.SceneName = sceneName;
            mapNetworkManager.StartClient(address, port, connectKey);
        }

        public bool IsConnectedToCentralServer()
        {
            return centralNetworkManager.IsClientConnected;
        }

        public void RequestUserLogin(string username, string password, AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserLogin(username, password, callback);
        }

        public void RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserRegister(username, password, callback);
        }

        public void RequestUserLogout(AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserLogout(callback);
        }

        public void RequestCharacters(AckMessageCallback callback)
        {
            centralNetworkManager.RequestCharacters(callback);
        }

        public void RequestCreateCharacter(string characterName, string databaseId, AckMessageCallback callback)
        {
            centralNetworkManager.RequestCreateCharacter(characterName, databaseId, callback);
        }

        public void RequestDeleteCharacter(string characterId, AckMessageCallback callback)
        {
            centralNetworkManager.RequestDeleteCharacter(characterId, callback);
        }

        public void RequestSelectCharacter(string characterId, AckMessageCallback callback)
        {
            centralNetworkManager.RequestSelectCharacter(characterId, callback);
        }
        #endregion
    }
}
