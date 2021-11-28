using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(-897)]
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        // User peers (Login / Register / Manager characters)
        protected readonly Dictionary<long, CentralUserPeerInfo> userPeers = new Dictionary<long, CentralUserPeerInfo>();
        protected readonly Dictionary<string, CentralUserPeerInfo> userPeersByUserId = new Dictionary<string, CentralUserPeerInfo>();
#endif
        [Header("Cluster")]
        public int clusterServerPort = 6001;

        [Header("Map Spawn")]
        public int mapSpawnMillisecondsTimeout = 0;

        [Header("User Account")]
        public bool disableDefaultLogin = false;
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        public bool requireEmail = false;
        public bool requireEmailVerification = false;

        public System.Action onClientConnected;
        public System.Action<DisconnectInfo> onClientDisconnected;

#if UNITY_STANDALONE && !CLIENT_BUILD
        public ClusterServer ClusterServer { get; private set; }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        protected override void Start()
        {
            base.Start();
#if UNITY_STANDALONE && !CLIENT_BUILD
            ClusterServer = new ClusterServer(this);
#endif
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Requests
            RegisterRequestToServer<RequestUserLoginMessage, ResponseUserLoginMessage>(MMORequestTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterRequestToServer<RequestUserRegisterMessage, ResponseUserRegisterMessage>(MMORequestTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterRequestToServer<EmptyMessage, EmptyMessage>(MMORequestTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterRequestToServer<EmptyMessage, ResponseCharactersMessage>(MMORequestTypes.RequestCharacters, HandleRequestCharacters);
            RegisterRequestToServer<RequestCreateCharacterMessage, ResponseCreateCharacterMessage>(MMORequestTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterRequestToServer<RequestDeleteCharacterMessage, ResponseDeleteCharacterMessage>(MMORequestTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
            RegisterRequestToServer<RequestSelectCharacterMessage, ResponseSelectCharacterMessage>(MMORequestTypes.RequestSelectCharacter, HandleRequestSelectCharacter);
            RegisterRequestToServer<RequestValidateAccessTokenMessage, ResponseValidateAccessTokenMessage>(MMORequestTypes.RequestValidateAccessToken, HandleRequestValidateAccessToken);
            // Keeping `RegisterClientMessages` and `RegisterServerMessages` for backward compatibility, can use any of below dev extension methods
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            this.InvokeInstanceDevExtMethods("RegisterMessages");
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
#if UNITY_STANDALONE && !CLIENT_BUILD
            userPeers.Clear();
            userPeersByUserId.Clear();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
            ClusterServer.StartServer();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
            ClusterServer.StopServer();
        }
#endif

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(connectionId, disconnectInfo);
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Remove disconnect user
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(connectionId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(connectionId);
            }
#endif
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            if (onClientConnected != null)
                onClientConnected.Invoke();
        }

        public override void OnClientDisconnected(DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(disconnectInfo);
            if (onClientDisconnected != null)
                onClientDisconnected.Invoke(disconnectInfo);
        }

        public bool MapContainsUser(string userId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            return ClusterServer.MapContainsUser(userId);
#else
            return false;
#endif
        }
    }
}
