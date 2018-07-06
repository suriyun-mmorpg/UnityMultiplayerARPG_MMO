using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        protected readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        protected readonly Dictionary<string, uint> spawningMapAcks = new Dictionary<string, uint>();
        protected readonly Dictionary<long, CentralServerPeerInfo> mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        protected readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersBySceneName = new Dictionary<string, CentralServerPeerInfo>();
        protected readonly Dictionary<long, CentralServerPeerInfo> chatServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        protected readonly Dictionary<long, CentralUserPeerInfo> userPeers = new Dictionary<long, CentralUserPeerInfo>();
        protected readonly Dictionary<string, CentralUserPeerInfo> userPeersByUserId = new Dictionary<string, CentralUserPeerInfo>();
        protected readonly Dictionary<long, HashSet<string>> mapUserIds = new Dictionary<long, HashSet<string>>();

        [Header("Account configuration")]
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        
        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;

        public BaseDatabase Database
        {
            get { return MMOServerInstance.Singleton.Database; }
        }
        
        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterServerMessages()
        {
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
            RegisterServerMessage(MMOMessageTypes.ResponseSpawnMap, HandleResponseSpawnMap);
            RegisterServerMessage(MMOMessageTypes.RequestValidateAccessToken, HandleRequestValidateAccessToken);
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            RegisterServerMessage(MMOMessageTypes.RequestFacebookLogin, HandleRequestFacebookLogin);
        }

        protected override void RegisterClientMessages()
        {
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

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(peer, disconnectInfo);
            mapSpawnServerPeers.Remove(peer.ConnectId);
            CentralServerPeerInfo mapServerPeerInfo;
            if (mapServerPeers.TryGetValue(peer.ConnectId, out mapServerPeerInfo))
            {
                mapServerPeersBySceneName.Remove(mapServerPeerInfo.extra);
                mapServerPeers.Remove(peer.ConnectId);
                mapUserIds.Remove(peer.ConnectId);
            }
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(peer.ConnectId);
            }
        }

        public override void OnClientConnected(NetPeer peer)
        {
            base.OnClientConnected(peer);
            if (onClientConnected != null)
                onClientConnected(peer);
        }

        public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(peer, disconnectInfo);
            if (onClientDisconnected != null)
                onClientDisconnected(peer, disconnectInfo);
        }

        public bool MapContainsUser(string userId)
        {
            foreach (var mapUser in mapUserIds.Values)
            {
                if (mapUser.Contains(userId))
                    return true;
            }
            return false;
        }
    }
}
