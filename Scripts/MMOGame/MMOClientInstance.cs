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
        public static string UserId { get; private set; }
        public static string AccessToken { get; private set; }
        public static string SelectCharacterId { get; private set; }

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
            centralNetworkManager.RequestUserLogin(username, password, (responseCode, messageData) => OnRequestUserLogin(responseCode, messageData, callback));
        }

        public void RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserRegister(username, password, callback);
        }

        public void RequestUserLogout(AckMessageCallback callback)
        {
            centralNetworkManager.RequestUserLogout((responseCode, messageData) => OnRequestUserLogout(responseCode, messageData, callback));
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
            centralNetworkManager.RequestSelectCharacter(characterId, (responseCode, messageData) => OnRequestSelectCharacter(responseCode, messageData, characterId, callback));
        }

        private void OnRequestUserLogin(AckResponseCode responseCode, BaseAckMessage messageData, AckMessageCallback callback)
        {
            if (callback != null)
                callback(responseCode, messageData);

            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
            var castedMessage = messageData as ResponseUserLoginMessage;
            if (castedMessage.responseCode == AckResponseCode.Success)
            {
                UserId = castedMessage.userId;
                AccessToken = castedMessage.accessToken;
                SelectCharacterId = string.Empty;
            }
        }

        private void OnRequestUserLogout(AckResponseCode responseCode, BaseAckMessage messageData, AckMessageCallback callback)
        {
            if (callback != null)
                callback(responseCode, messageData);

            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
        }

        private void OnRequestSelectCharacter(AckResponseCode responseCode, BaseAckMessage messageData, string characterId, AckMessageCallback callback)
        {
            if (callback != null)
                callback(responseCode, messageData);

            SelectCharacterId = string.Empty;
            var castedMessage = messageData as ResponseSelectCharacterMessage;
            if (castedMessage.responseCode == AckResponseCode.Success)
                SelectCharacterId = characterId;
        }
        #endregion
    }
}
