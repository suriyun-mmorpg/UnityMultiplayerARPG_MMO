using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersByMapName = new Dictionary<string, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralUserPeerInfo> userPeers = new Dictionary<long, CentralUserPeerInfo>();
        public readonly Dictionary<string, CentralUserPeerInfo> userPeersByUserId = new Dictionary<string, CentralUserPeerInfo>();

        [Header("Server configuration")]
        public BaseDatabase database;
        [Header("Account configuration")]
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;

        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;
        
        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            RegisterServerMessage(MessageTypes.RequestAppServerRegister, HandleRequestAppServerRegister);
            RegisterServerMessage(MessageTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
            RegisterServerMessage(MessageTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterServerMessage(MessageTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterServerMessage(MessageTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterServerMessage(MessageTypes.RequestCharacters, HandleRequestCharacters);
            RegisterServerMessage(MessageTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterServerMessage(MessageTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
            RegisterServerMessage(MessageTypes.RequestSelectCharacter, HandleRequestSelectCharacter);
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterClientMessage(MessageTypes.ResponseAppServerRegister, HandleResponseAppServerRegister);
            RegisterClientMessage(MessageTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
            RegisterClientMessage(MessageTypes.ResponseUserLogin, HandleResponseUserLogin);
            RegisterClientMessage(MessageTypes.ResponseUserRegister, HandleResponseUserRegister);
            RegisterClientMessage(MessageTypes.ResponseUserLogout, HandleResponseUserLogout);
            RegisterClientMessage(MessageTypes.ResponseCharacters, HandleResponseCharacters);
            RegisterClientMessage(MessageTypes.ResponseCreateCharacter, HandleResponseCreateCharacter);
            RegisterClientMessage(MessageTypes.ResponseDeleteCharacter, HandleResponseDeleteCharacter);
            RegisterClientMessage(MessageTypes.ResponseSelectCharacter, HandleResponseSelectCharacter);
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(peer, disconnectInfo);
            mapSpawnServerPeers.Remove(peer.ConnectId);
            CentralServerPeerInfo mapServerPeerInfo;
            if (mapServerPeers.TryGetValue(peer.ConnectId, out mapServerPeerInfo))
            {
                mapServerPeersByMapName.Remove(mapServerPeerInfo.extra);
                mapServerPeers.Remove(peer.ConnectId);
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
    }
}
