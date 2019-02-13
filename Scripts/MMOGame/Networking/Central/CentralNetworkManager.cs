using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        protected readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        protected readonly Dictionary<string, uint> spawningMapAcks = new Dictionary<string, uint>();
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
        // Ack Id / Map-Server Transport Handler dictionary
        private readonly Dictionary<uint, TransportHandler> requestSpawnMapTransportHandlers = new Dictionary<uint, TransportHandler>();
        private readonly Dictionary<uint, uint> requestSpawnMapAckIds = new Dictionary<uint, uint>();

        [Header("Account configuration")]
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        
        public System.Action onClientConnected;
        public System.Action<DisconnectInfo> onClientDisconnected;

        public BaseDatabase Database
        {
            get { return MMOServerInstance.Singleton.Database; }
        }

        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterClientMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            base.RegisterClientMessages();
            RegisterClientMessage(MMOMessageTypes.ResponseAppServerRegister, HandleResponseAppServerRegister);
            RegisterClientMessage(MMOMessageTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
            RegisterClientMessage(MMOMessageTypes.ResponseUserLogin, HandleResponseUserLogin);
            RegisterClientMessage(MMOMessageTypes.ResponseUserRegister, HandleResponseUserRegister);
            RegisterClientMessage(MMOMessageTypes.ResponseUserLogout, HandleResponseUserLogout);
            RegisterClientMessage(MMOMessageTypes.ResponseCharacters, HandleResponseCharacters);
            RegisterClientMessage(MMOMessageTypes.ResponseCreateCharacter, HandleResponseCreateCharacter);
            RegisterClientMessage(MMOMessageTypes.ResponseDeleteCharacter, HandleResponseDeleteCharacter);
            RegisterClientMessage(MMOMessageTypes.ResponseSelectCharacter, HandleResponseSelectCharacter);
            RegisterClientMessage(MMOMessageTypes.ResponseValidateAccessToken, HandleResponseValidateAccessToken);
        }

        protected override void RegisterServerMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            base.RegisterServerMessages();
            RegisterServerMessage(MMOMessageTypes.RequestAppServerRegister, HandleRequestAppServerRegister);
            RegisterServerMessage(MMOMessageTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
            RegisterServerMessage(MMOMessageTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterServerMessage(MMOMessageTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterServerMessage(MMOMessageTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterServerMessage(MMOMessageTypes.RequestCharacters, HandleRequestCharacters);
            RegisterServerMessage(MMOMessageTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterServerMessage(MMOMessageTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
            RegisterServerMessage(MMOMessageTypes.RequestSelectCharacter, HandleRequestSelectCharacter);
            RegisterServerMessage(MMOMessageTypes.RequestSpawnMap, HandleRequestSpawnMap);
            RegisterServerMessage(MMOMessageTypes.ResponseSpawnMap, HandleResponseSpawnMap);
            RegisterServerMessage(MMOMessageTypes.RequestValidateAccessToken, HandleRequestValidateAccessToken);
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            RegisterServerMessage(MMOMessageTypes.RequestFacebookLogin, HandleRequestFacebookLogin);
            RegisterServerMessage(MMOMessageTypes.RequestGooglePlayLogin, HandleRequestGooglePlayLogin);
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            mapSpawnServerPeers.Clear();
            spawningMapAcks.Clear();
            mapServerPeers.Clear();
            mapServerPeersBySceneName.Clear();
            instanceMapServerPeers.Clear();
            instanceMapServerPeersByInstanceId.Clear();
            chatServerPeers.Clear();
            userPeers.Clear();
            userPeersByUserId.Clear();
            mapUserIds.Clear();
            requestSpawnMapTransportHandlers.Clear();
            requestSpawnMapAckIds.Clear();
        }

        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
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
            foreach (HashSet<string> mapUser in mapUserIds.Values)
            {
                if (mapUser.Contains(userId))
                    return true;
            }
            return false;
        }
    }
}
