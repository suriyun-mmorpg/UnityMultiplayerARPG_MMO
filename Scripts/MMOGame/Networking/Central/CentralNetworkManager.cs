using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
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
        // <Ack Id, <Map-Server Transport Handler, Map-Server Ack Id> dictionary
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

        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterClientMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            base.RegisterClientMessages();
            EnableClientRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Responses
            RegisterClientResponse<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister);
            RegisterClientResponse<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress);
            RegisterClientResponse<RequestUserLoginMessage, ResponseUserLoginMessage>(MMORequestTypes.RequestUserLogin);
            RegisterClientResponse<RequestUserRegisterMessage, ResponseUserRegisterMessage>(MMORequestTypes.RequestUserRegister);
            RegisterClientResponse<EmptyMessage, EmptyMessage>(MMORequestTypes.RequestUserLogout);
            RegisterClientResponse<EmptyMessage, ResponseCharactersMessage>(MMORequestTypes.RequestCharacters);
            RegisterClientResponse<RequestCreateCharacterMessage, ResponseCreateCharacterMessage>(MMORequestTypes.RequestCreateCharacter);
            RegisterClientResponse<RequestDeleteCharacterMessage, ResponseDeleteCharacterMessage>(MMORequestTypes.RequestDeleteCharacter);
            RegisterClientResponse<RequestSelectCharacterMessage, ResponseSelectCharacterMessage>(MMORequestTypes.RequestSelectCharacter);
            RegisterClientResponse<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap);
            RegisterClientResponse<RequestValidateAccessTokenMessage, ResponseValidateAccessTokenMessage>(MMORequestTypes.RequestValidateAccessToken);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected override void RegisterServerMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            base.RegisterServerMessages();
            EnableServerRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            // Requests
            RegisterServerRequest<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, HandleRequestAppServerRegister);
            RegisterServerRequest<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
            RegisterServerRequest<RequestUserLoginMessage, ResponseUserLoginMessage>(MMORequestTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterServerRequest<RequestUserRegisterMessage, ResponseUserRegisterMessage>(MMORequestTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterServerRequest<EmptyMessage, EmptyMessage>(MMORequestTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterServerRequest<EmptyMessage, ResponseCharactersMessage>(MMORequestTypes.RequestCharacters, HandleRequestCharacters);
            RegisterServerRequest<RequestCreateCharacterMessage, ResponseCreateCharacterMessage>(MMORequestTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterServerRequest<RequestDeleteCharacterMessage, ResponseDeleteCharacterMessage>(MMORequestTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
            RegisterServerRequest<RequestSelectCharacterMessage, ResponseSelectCharacterMessage>(MMORequestTypes.RequestSelectCharacter, HandleRequestSelectCharacter);
            RegisterServerRequest<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleRequestSpawnMap);
            RegisterServerRequest<RequestValidateAccessTokenMessage, ResponseValidateAccessTokenMessage>(MMORequestTypes.RequestValidateAccessToken, HandleRequestValidateAccessToken);
            // Responses
            RegisterServerResponse<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleResponseSpawnMap);
        }
#endif

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

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(connectionId, disconnectInfo);
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
        }
#endif

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

#if UNITY_STANDALONE && !CLIENT_BUILD
        public bool MapContainsUser(string userId)
        {
            foreach (HashSet<string> mapUser in mapUserIds.Values)
            {
                if (mapUser.Contains(userId))
                    return true;
            }
            return false;
        }
#endif
    }
}
