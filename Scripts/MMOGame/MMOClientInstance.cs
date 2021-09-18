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

        [Header("Client Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager = null;
        [SerializeField]
        private MapNetworkManager mapNetworkManager = null;

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;
        [SerializeField]
        private MmoNetworkSetting[] networkSettings = new MmoNetworkSetting[0];

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
            GameInstance.UserId = string.Empty;
            GameInstance.UserToken = string.Empty;
            GameInstance.SelectedCharacterId = string.Empty;
        }

        public void RequestUserLogin(string username, string password, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            centralNetworkManager.RequestUserLogin(username, password, (responseHandler, responseCode, response) => OnRequestUserLogin(responseHandler, responseCode, response, callback).Forget());
        }

        public void RequestUserRegister(string username, string password, ResponseDelegate<ResponseUserRegisterMessage> callback)
        {
            centralNetworkManager.RequestUserRegister(username, password, callback);
        }

        public void RequestUserLogout(ResponseDelegate<INetSerializable> callback)
        {
            centralNetworkManager.RequestUserLogout((responseHandler, responseCode, response) => OnRequestUserLogout(responseHandler, responseCode, response, callback).Forget());
        }

        public void RequestValidateAccessToken(string userId, string accessToken, ResponseDelegate<ResponseValidateAccessTokenMessage> callback)
        {
            centralNetworkManager.RequestValidateAccessToken(userId, accessToken, (responseHandler, responseCode, response) => OnRequestValidateAccessToken(responseHandler, responseCode, response, callback).Forget());
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
            centralNetworkManager.RequestSelectCharacter(characterId, (responseHandler, responseCode, response) => OnRequestSelectCharacter(responseHandler, responseCode, response, characterId, callback).Forget());
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
                GameInstance.SelectedCharacterId = string.Empty;
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
