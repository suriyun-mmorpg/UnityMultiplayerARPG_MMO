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
        // Client data, May keep these data in player prefs to do auto login system
        public static string SelectedCentralAddress { get; private set; }
        public static int SelectedCentralPort { get; private set; }
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
            centralNetworkManager.networkAddress = SelectedCentralAddress = address;
            centralNetworkManager.networkPort = SelectedCentralPort = port;
            StartCentralClient();
        }

        public void StopCentralClient()
        {
            centralNetworkManager.StopClient();
        }

        public void StartMapClient(string sceneName, string address, int port, string connectKey)
        {
            mapNetworkManager.Assets.onlineScene.SceneName = sceneName;
            mapNetworkManager.StartClient(address, port, connectKey);
        }

        public void StopMapClient()
        {
            mapNetworkManager.StopClient();
        }

        public bool IsConnectedToCentralServer()
        {
            return centralNetworkManager.IsClientConnected;
        }

        public NetPeer GetCentralClientPeer()
        {
            return centralNetworkManager.Client.Peer;
        }

        public void ClearClientData()
        {
            SelectedCentralAddress = string.Empty;
            SelectedCentralPort = 0;
            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
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

        public void RequestValidateAccessToken(string userId, string accessToken, AckMessageCallback callback)
        {
            centralNetworkManager.RequestValidateAccessToken(userId, accessToken, (responseCode, messageData) => OnRequestValidateAccessToken(responseCode, messageData, callback));
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

        private void OnRequestValidateAccessToken(AckResponseCode responseCode, BaseAckMessage messageData, AckMessageCallback callback)
        {
            if (callback != null)
                callback(responseCode, messageData);

            UserId = string.Empty;
            AccessToken = string.Empty;
            var castedMessage = messageData as ResponseValidateAccessTokenMessage;
            if (castedMessage.responseCode == AckResponseCode.Success)
            {
                UserId = castedMessage.userId;
                AccessToken = castedMessage.accessToken;
            }
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
