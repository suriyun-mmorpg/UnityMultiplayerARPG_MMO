using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(-898)]
    public partial class MMOClientInstance : MonoBehaviour
    {
        public static MMOClientInstance Singleton { get; protected set; }
        // Client data, May keep these data in player prefs to do auto login system
        public static string SelectedCentralAddress { get; private set; }
        public static int SelectedCentralPort { get; private set; }

        [Header("Client Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager = null;
        [SerializeField]
        private MapNetworkManager mapNetworkManager = null;

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;
        [SerializeField]
        private bool webSocketSecure = false;
        [SerializeField]
        private MmoNetworkSetting[] networkSettings = new MmoNetworkSetting[0];

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public bool UseWebSocket { get { return useWebSocket; } }
        public bool WebSocketSecure { get { return webSocketSecure; } }
        public MmoNetworkSetting[] NetworkSettings { get { return networkSettings; } }

        public System.Action onCentralClientConnected;
        public System.Action<DisconnectReason, SocketError, UITextKeys> onCentralClientDisconnected;
        public System.Action onCentralClientStopped;

        public System.Action onMapClientConnected;
        public System.Action<DisconnectReason, SocketError, UITextKeys> onMapClientDisconnected;
        public System.Action onMapClientStopped;

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
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
        }

        private void OnEnable()
        {
            CentralNetworkManager.onClientConnected += OnCentralConnected;
            CentralNetworkManager.onClientDisconnected += OnCentralDisconnected;
            CentralNetworkManager.onClientStopped += OnCentralStopped;
            ClientGenericActions.onClientConnected += OnMapConnected;
            ClientGenericActions.onClientDisconnected += OnMapDisconnected;
            ClientGenericActions.onClientStopped += OnMapStopped;
        }

        private void OnDisable()
        {
            CentralNetworkManager.onClientConnected -= OnCentralConnected;
            CentralNetworkManager.onClientDisconnected -= OnCentralDisconnected;
            CentralNetworkManager.onClientStopped -= OnCentralStopped;
            ClientGenericActions.onClientConnected -= OnMapConnected;
            ClientGenericActions.onClientDisconnected -= OnMapDisconnected;
            ClientGenericActions.onClientStopped -= OnMapStopped;
        }

        public void OnCentralConnected()
        {
            if (onCentralClientConnected != null)
                onCentralClientConnected.Invoke();
        }

        public void OnCentralDisconnected(DisconnectReason reason, SocketError socketError, UITextKeys message)
        {
            if (onCentralClientDisconnected != null)
                onCentralClientDisconnected.Invoke(reason, socketError, message);
            ClearClientData();
        }

        public void OnCentralStopped()
        {
            if (onCentralClientStopped != null)
                onCentralClientStopped.Invoke();
        }

        public void OnMapConnected()
        {
            if (onMapClientConnected != null)
                onMapClientConnected.Invoke();
        }

        public void OnMapDisconnected(DisconnectReason reason, SocketError socketError, UITextKeys message)
        {
            if (onMapClientDisconnected != null)
                onMapClientDisconnected.Invoke(reason, socketError, message);
        }

        public void OnMapStopped()
        {
            if (onMapClientStopped != null)
                onMapClientStopped.Invoke();
            // Restart central client after exit from map-server to login and go to character management scene
            if (!IsConnectedToCentralServer())
                StartCentralClient();
        }

        #region Client functions
        public void StartCentralClient()
        {
            CentralNetworkManager.useWebSocket = UseWebSocket;
            CentralNetworkManager.webSocketSecure = WebSocketSecure;
            CentralNetworkManager.StartClient();
        }

        public void StartCentralClient(string address, int port)
        {
            CentralNetworkManager.useWebSocket = UseWebSocket;
            CentralNetworkManager.webSocketSecure = WebSocketSecure;
            CentralNetworkManager.StartClient(address, port);
        }

        public void StopCentralClient()
        {
            CentralNetworkManager.StopClient();
        }

        public void StartMapClient(string sceneName, string address, int port)
        {
            MapNetworkManager.Assets.onlineScene.SceneName = sceneName;
            MapNetworkManager.useWebSocket = UseWebSocket;
            MapNetworkManager.webSocketSecure = WebSocketSecure;
            MapNetworkManager.StartClient(address, port);
        }

        public void StopMapClient()
        {
            MapNetworkManager.StopClient();
        }

        public bool IsConnectedToCentralServer()
        {
            return CentralNetworkManager.IsClientConnected;
        }

        public void ClearClientData()
        {
            SelectedCentralAddress = string.Empty;
            SelectedCentralPort = 0;
            GameInstance.UserId = string.Empty;
            GameInstance.UserToken = string.Empty;
            GameInstance.SelectedCharacterId = string.Empty;
        }

        public void RequestUserLogin(string username, string password, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            CentralNetworkManager.RequestUserLogin(username, password, (responseHandler, responseCode, response) => OnRequestUserLogin(responseHandler, responseCode, response, callback).Forget());
        }

        public void RequestUserRegister(string username, string password, string email, ResponseDelegate<ResponseUserRegisterMessage> callback)
        {
            CentralNetworkManager.RequestUserRegister(username, password, email, callback);
        }

        public void RequestUserLogout(ResponseDelegate<INetSerializable> callback)
        {
            CentralNetworkManager.RequestUserLogout((responseHandler, responseCode, response) => OnRequestUserLogout(responseHandler, responseCode, response, callback).Forget());
        }

        public void RequestValidateAccessToken(string userId, string accessToken, ResponseDelegate<ResponseValidateAccessTokenMessage> callback)
        {
            CentralNetworkManager.RequestValidateAccessToken(userId, accessToken, (responseHandler, responseCode, response) => OnRequestValidateAccessToken(responseHandler, responseCode, response, callback).Forget());
        }

        public void RequestCharacters(ResponseDelegate<ResponseCharactersMessage> callback)
        {
            CentralNetworkManager.RequestCharacters(callback);
        }

        public void RequestCreateCharacter(PlayerCharacterData characterData, ResponseDelegate<ResponseCreateCharacterMessage> callback)
        {
            CentralNetworkManager.RequestCreateCharacter(characterData, callback);
        }

        public void RequestDeleteCharacter(string characterId, ResponseDelegate<ResponseDeleteCharacterMessage> callback)
        {
            CentralNetworkManager.RequestDeleteCharacter(characterId, callback);
        }

        public void RequestSelectCharacter(string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback)
        {
            CentralNetworkManager.RequestSelectCharacter(characterId, (responseHandler, responseCode, response) => OnRequestSelectCharacter(responseHandler, responseCode, response, characterId, callback).Forget());
        }

        private async UniTaskVoid OnRequestUserLogin(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserLoginMessage response, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            GameInstance.UserId = string.Empty;
            GameInstance.UserToken = string.Empty;
            GameInstance.SelectedCharacterId = string.Empty;
            if (responseCode == AckResponseCode.Success)
            {
                GameInstance.UserId = response.userId;
                GameInstance.UserToken = response.accessToken;
            }
        }

        private async UniTaskVoid OnRequestUserLogout(ResponseHandlerData responseHandler, AckResponseCode responseCode, INetSerializable response, ResponseDelegate<INetSerializable> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            GameInstance.UserId = string.Empty;
            GameInstance.UserToken = string.Empty;
            GameInstance.SelectedCharacterId = string.Empty;
        }

        private async UniTaskVoid OnRequestValidateAccessToken(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseValidateAccessTokenMessage response, ResponseDelegate<ResponseValidateAccessTokenMessage> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            GameInstance.UserId = string.Empty;
            GameInstance.UserToken = string.Empty;
            if (responseCode == AckResponseCode.Success)
            {
                GameInstance.UserId = response.userId;
                GameInstance.UserToken = response.accessToken;
            }
        }

        private async UniTaskVoid OnRequestSelectCharacter(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSelectCharacterMessage response, string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback)
        {
            await UniTask.Yield();

            if (callback != null)
                callback.Invoke(responseHandler, responseCode, response);

            GameInstance.SelectedCharacterId = string.Empty;
            if (responseCode == AckResponseCode.Success)
            {
                GameInstance.SelectedCharacterId = characterId;
            }
        }
        #endregion
    }
}
