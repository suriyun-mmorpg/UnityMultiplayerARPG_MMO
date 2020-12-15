using System.Net;
using System.Net.Security;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;

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
            ClientGenericActions.onClientConnected += OnMapServerConnected;
            ClientGenericActions.onClientDisconnected += OnMapServerDisconnected;
        }

        private void OnDisable()
        {
            centralNetworkManager.onClientConnected -= OnCentralServerConnected;
            centralNetworkManager.onClientDisconnected -= OnCentralServerDisconnected;
            ClientGenericActions.onClientConnected -= OnMapServerConnected;
            ClientGenericActions.onClientDisconnected -= OnMapServerDisconnected;
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

        public void RequestUserLogin(string username, string password, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            centralNetworkManager.RequestUserLogin(username, password, (responseHandler, responseCode, response) => OnRequestUserLogin(responseHandler, responseCode, response, callback));
        }

        public void RequestUserRegister(string username, string password, ResponseDelegate<ResponseUserRegisterMessage> callback)
        {
            centralNetworkManager.RequestUserRegister(username, password, callback);
        }

        public void RequestUserLogout(ResponseDelegate callback)
        {
            centralNetworkManager.RequestUserLogout((responseHandler, responseCode, response) => OnRequestUserLogout(responseHandler, responseCode, response, callback));
        }

        public void RequestValidateAccessToken(string userId, string accessToken, ResponseDelegate<ResponseValidateAccessTokenMessage> callback)
        {
            centralNetworkManager.RequestValidateAccessToken(userId, accessToken, (responseHandler, responseCode, response) => OnRequestValidateAccessToken(responseHandler, responseCode, response, callback));
        }

        public void RequestCharacters(ResponseDelegate<ResponseCharactersMessage> callback)
        {
            centralNetworkManager.RequestCharacters(callback);
        }

        public void RequestCreateCharacter(PlayerCharacterData characterData, ResponseDelegate<ResponseCreateCharacterMessage> callback)
        {
            centralNetworkManager.RequestCreateCharacter(characterData, callback);
        }

        public void RequestDeleteCharacter(string characterId, ResponseDelegate<ResponseDeleteCharacterMessage> callback)
        {
            centralNetworkManager.RequestDeleteCharacter(characterId, callback);
        }

        public void RequestSelectCharacter(string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback)
        {
            centralNetworkManager.RequestSelectCharacter(characterId, (responseHandler, responseCode, response) => OnRequestSelectCharacter(responseHandler, responseCode, response, characterId, callback));
        }

        private async UniTaskVoid OnRequestUserLogin(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserLoginMessage response, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
            if (responseCode == AckResponseCode.Success)
            {
                UserId = response.userId;
                AccessToken = response.accessToken;
                SelectCharacterId = string.Empty;
            }
        }

        private async UniTaskVoid OnRequestUserLogout(ResponseHandlerData responseHandler, AckResponseCode responseCode, INetSerializable response, ResponseDelegate callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            UserId = string.Empty;
            AccessToken = string.Empty;
            SelectCharacterId = string.Empty;
        }

        private async UniTaskVoid OnRequestValidateAccessToken(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseValidateAccessTokenMessage response, ResponseDelegate<ResponseValidateAccessTokenMessage> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            UserId = string.Empty;
            AccessToken = string.Empty;
            if (responseCode == AckResponseCode.Success)
            {
                UserId = response.userId;
                AccessToken = response.accessToken;
            }
        }

        private async UniTaskVoid OnRequestSelectCharacter(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSelectCharacterMessage response, string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            SelectCharacterId = string.Empty;
            if (responseCode == AckResponseCode.Success)
            {
                SelectCharacterId = characterId;
            }
        }
        #endregion
    }
}
