using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class MMOClientInstance : MonoBehaviour
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

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;
        [SerializeField]
        private MmoNetworkSetting[] networkSettings;

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public bool UseWebSocket { get { return useWebSocket; } }
        public MmoNetworkSetting[] NetworkSettings { get { return networkSettings; } }

        public System.Action onCentralClientConnected;
        public System.Action<DisconnectInfo> onCentralClientDisconnected;

        public System.Action onMapClientConnected;
        public System.Action<DisconnectInfo> onMapClientDisconnected;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

            // Active WebSockets
            CentralNetworkManager.useWebSocket = UseWebSocket;
            MapNetworkManager.useWebSocket = UseWebSocket;
        }

        private void OnEnable()
        {
            centralNetworkManager.onClientConnected += OnCentralServerConnected;
            centralNetworkManager.onClientDisconnected += OnCentralServerDisconnected;
            mapNetworkManager.onClientConnected += OnMapServerConnected;
            mapNetworkManager.onClientDisconnected += OnMapServerDisconnected;
        }

        private void OnDisable()
        {
            centralNetworkManager.onClientConnected -= OnCentralServerConnected;
            centralNetworkManager.onClientDisconnected -= OnCentralServerDisconnected;
            mapNetworkManager.onClientConnected -= OnMapServerConnected;
            mapNetworkManager.onClientDisconnected -= OnMapServerDisconnected;
        }

        public void OnCentralServerConnected()
        {
            if (onCentralClientConnected != null)
                onCentralClientConnected.Invoke();
        }

        public void OnCentralServerDisconnected(DisconnectInfo disconnectInfo)
        {
            if (onCentralClientDisconnected != null)
                onCentralClientDisconnected.Invoke(disconnectInfo);
        }

        public void OnMapServerConnected()
        {
            if (onMapClientConnected != null)
                onMapClientConnected.Invoke();
            // Disconnect from central server when connected to map server
            StopCentralClient();
        }

        public void OnMapServerDisconnected(DisconnectInfo disconnectInfo)
        {
            if (onMapClientDisconnected != null)
                onMapClientDisconnected.Invoke(disconnectInfo);
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

        public void StartMapClient(string sceneName, string address, int port)
        {
            mapNetworkManager.Assets.onlineScene.SceneName = sceneName;
            mapNetworkManager.StartClient(address, port);
        }

        public void StopMapClient()
        {
            mapNetworkManager.StopClient();
        }

        public bool IsConnectedToCentralServer()
        {
            return centralNetworkManager.IsClientConnected;
        }

        public void ClearClientData()
        {
            SelectedCentralAddress = string.Empty;
            SelectedCentralPort = 0;
            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
        }

        public void RequestUserLogin(string username, string password, AckMessageCallback<ResponseUserLoginMessage> callback)
        {
            centralNetworkManager.RequestUserLogin(username, password, (messageData) => OnRequestUserLogin(messageData, callback));
        }

        public void RequestUserRegister(string username, string password, AckMessageCallback<ResponseUserRegisterMessage> callback)
        {
            centralNetworkManager.RequestUserRegister(username, password, callback);
        }

        public void RequestUserLogout(AckMessageCallback<BaseAckMessage> callback)
        {
            centralNetworkManager.RequestUserLogout((messageData) => OnRequestUserLogout(messageData, callback));
        }

        public void RequestValidateAccessToken(string userId, string accessToken, AckMessageCallback<ResponseValidateAccessTokenMessage> callback)
        {
            centralNetworkManager.RequestValidateAccessToken(userId, accessToken, (messageData) => OnRequestValidateAccessToken(messageData, callback));
        }

        public void RequestCharacters(AckMessageCallback<ResponseCharactersMessage> callback)
        {
            centralNetworkManager.RequestCharacters(callback);
        }

        public void RequestCreateCharacter(PlayerCharacterData characterData, AckMessageCallback<ResponseCreateCharacterMessage> callback)
        {
            centralNetworkManager.RequestCreateCharacter(characterData, callback);
        }

        public void RequestDeleteCharacter(string characterId, AckMessageCallback<ResponseDeleteCharacterMessage> callback)
        {
            centralNetworkManager.RequestDeleteCharacter(characterId, callback);
        }

        public void RequestSelectCharacter(string characterId, AckMessageCallback<ResponseSelectCharacterMessage> callback)
        {
            centralNetworkManager.RequestSelectCharacter(characterId, (messageData) => OnRequestSelectCharacter(messageData, characterId, callback));
        }

        private void OnRequestUserLogin(ResponseUserLoginMessage messageData, AckMessageCallback<ResponseUserLoginMessage> callback)
        {
            if (callback != null)
                callback.Invoke(messageData);

            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
            if (messageData.responseCode == AckResponseCode.Success)
            {
                UserId = messageData.userId;
                AccessToken = messageData.accessToken;
                SelectCharacterId = string.Empty;
            }
        }

        private void OnRequestUserLogout(BaseAckMessage messageData, AckMessageCallback<BaseAckMessage> callback)
        {
            if (callback != null)
                callback.Invoke(messageData);

            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
        }

        private void OnRequestValidateAccessToken(ResponseValidateAccessTokenMessage messageData, AckMessageCallback<ResponseValidateAccessTokenMessage> callback)
        {
            if (callback != null)
                callback.Invoke(messageData);

            UserId = string.Empty;
            AccessToken = string.Empty;
            if (messageData.responseCode == AckResponseCode.Success)
            {
                UserId = messageData.userId;
                AccessToken = messageData.accessToken;
            }
        }

        private void OnRequestSelectCharacter(ResponseSelectCharacterMessage messageData, string characterId, AckMessageCallback<ResponseSelectCharacterMessage> callback)
        {
            if (callback != null)
                callback.Invoke(messageData);

            SelectCharacterId = string.Empty;
            if (messageData.responseCode == AckResponseCode.Success)
            {
                SelectCharacterId = characterId;
            }
        }
        #endregion
    }
}
