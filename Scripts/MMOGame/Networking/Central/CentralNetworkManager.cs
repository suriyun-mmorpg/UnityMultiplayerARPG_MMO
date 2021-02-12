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
        protected readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        // Map server peers
        protected readonly Dictionary<long, CentralServerPeerInfo> mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        protected readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersBySceneName = new Dictionary<string, CentralServerPeerInfo>();
        // Instance map server peers
        protected readonly Dictionary<long, CentralServerPeerInfo> instanceMapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        protected readonly Dictionary<string, CentralServerPeerInfo> instanceMapServerPeersByInstanceId = new Dictionary<string, CentralServerPeerInfo>();
        // Chat server peers
        protected readonly Dictionary<long, CentralServerPeerInfo> chatServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        // User peers (Login / Register / Manager characters)
        protected readonly Dictionary<long, CentralUserPeerInfo> userPeers = new Dictionary<long, CentralUserPeerInfo>();
        protected readonly Dictionary<string, CentralUserPeerInfo> userPeersByUserId = new Dictionary<string, CentralUserPeerInfo>();
        // Map users, users whom connected to map server / instance map server will be kept in this list
        protected readonly Dictionary<long, HashSet<string>> mapUserIds = new Dictionary<long, HashSet<string>>();
        // <Request Id, Response Handler> dictionary
        protected readonly Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>> requestSpawnMapHandlers = new Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>>();
#endif

        [Header("Map Spawn")]
        public int mapSpawnMillisecondsTimeout = 0;

        [Header("User Account")]
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;

        public System.Action onClientConnected;
        public System.Action<DisconnectInfo> onClientDisconnected;

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Messages
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            // Requests
            RegisterRequestToServer<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, HandleRequestAppServerRegister);
            RegisterRequestToServer<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
            RegisterRequestToServer<RequestUserLoginMessage, ResponseUserLoginMessage>(MMORequestTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterRequestToServer<RequestUserRegisterMessage, ResponseUserRegisterMessage>(MMORequestTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterRequestToServer<EmptyMessage, EmptyMessage>(MMORequestTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterRequestToServer<EmptyMessage, ResponseCharactersMessage>(MMORequestTypes.RequestCharacters, HandleRequestCharacters);
            RegisterRequestToServer<RequestCreateCharacterMessage, ResponseCreateCharacterMessage>(MMORequestTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterRequestToServer<RequestDeleteCharacterMessage, ResponseDeleteCharacterMessage>(MMORequestTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
            RegisterRequestToServer<RequestSelectCharacterMessage, ResponseSelectCharacterMessage>(MMORequestTypes.RequestSelectCharacter, HandleRequestSelectCharacter);
            RegisterRequestToServer<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleRequestSpawnMap);
            RegisterRequestToServer<RequestValidateAccessTokenMessage, ResponseValidateAccessTokenMessage>(MMORequestTypes.RequestValidateAccessToken, HandleRequestValidateAccessToken);
            // Register this response for map-spawn requests
            Server.RegisterResponseHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleResponseSpawnMap);
            // Keeping `RegisterClientMessages` and `RegisterServerMessages` for backward compatibility, can use any of below dev extension methods
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            this.InvokeInstanceDevExtMethods("RegisterMessages");
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
#if UNITY_STANDALONE && !CLIENT_BUILD
            mapSpawnServerPeers.Clear();
            mapServerPeers.Clear();
            mapServerPeersBySceneName.Clear();
            instanceMapServerPeers.Clear();
            instanceMapServerPeersByInstanceId.Clear();
            chatServerPeers.Clear();
            userPeers.Clear();
            userPeersByUserId.Clear();
            mapUserIds.Clear();
            requestSpawnMapHandlers.Clear();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
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
            // Remove disconnect map spawn server
            mapSpawnServerPeers.Remove(connectionId);
            // Remove disconnect map server
            CentralServerPeerInfo mapServerPeerInfo;
            if (mapServerPeers.TryGetValue(connectionId, out mapServerPeerInfo))
            {
                mapServerPeersBySceneName.Remove(mapServerPeerInfo.extra);
                mapServerPeers.Remove(connectionId);
                mapUserIds.Remove(connectionId);
            }
            // Remove disconnect instance map server
            CentralServerPeerInfo instanceMapServerPeerInfo;
            if (instanceMapServerPeers.TryGetValue(connectionId, out instanceMapServerPeerInfo))
            {
                instanceMapServerPeersByInstanceId.Remove(instanceMapServerPeerInfo.extra);
                instanceMapServerPeers.Remove(connectionId);
                mapUserIds.Remove(connectionId);
            }
            // Remove disconnect chat server
            chatServerPeers.Remove(connectionId);
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
            foreach (HashSet<string> mapUser in mapUserIds.Values)
            {
                if (mapUser.Contains(userId))
                    return true;
            }
#endif
            return false;
        }
    }
}
