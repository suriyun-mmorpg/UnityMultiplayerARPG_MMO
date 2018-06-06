using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public class CentralMsgTypes
        {
            public const short RequestAppServerRegister = 0;
            public const short ResponseAppServerRegister = 1;
            public const short RequestAppServerAddress = 2;
            public const short ResponseAppServerAddress = 3;
            public const short RequestUserLogin = 4;
            public const short ResponseUserLogin = 5;
            public const short RequestUserRegister = 6;
            public const short ResponseUserRegister = 7;
            public const short RequestUserLogout = 8;
            public const short ResponseUserLogout = 9;
            public const short RequestCharacters = 10;
            public const short ResponseCharacters = 11;
            public const short RequestCreateCharacter = 12;
            public const short ResponseCreateCharacter = 13;
            public const short RequestDeleteCharacter = 14;
            public const short ResponseDeleteCharacter = 15;
        }

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
            RegisterServerMessage(CentralMsgTypes.RequestAppServerRegister, HandleRequestAppServerRegister);
            RegisterServerMessage(CentralMsgTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
            RegisterServerMessage(CentralMsgTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterServerMessage(CentralMsgTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterServerMessage(CentralMsgTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterServerMessage(CentralMsgTypes.RequestCharacters, HandleRequestCharacters);
            RegisterServerMessage(CentralMsgTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterServerMessage(CentralMsgTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterClientMessage(CentralMsgTypes.ResponseAppServerRegister, HandleResponseAppServerRegister);
            RegisterClientMessage(CentralMsgTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
            RegisterClientMessage(CentralMsgTypes.ResponseUserLogin, HandleResponseUserLogin);
            RegisterClientMessage(CentralMsgTypes.ResponseUserRegister, HandleResponseUserRegister);
            RegisterClientMessage(CentralMsgTypes.ResponseUserLogout, HandleResponseUserLogout);
            RegisterClientMessage(CentralMsgTypes.ResponseCharacters, HandleResponseCharacters);
            RegisterClientMessage(CentralMsgTypes.ResponseCreateCharacter, HandleResponseCreateCharacter);
            RegisterClientMessage(CentralMsgTypes.ResponseDeleteCharacter, HandleResponseDeleteCharacter);
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

        public override void OnStartServer()
        {
            base.OnStartServer();
            database.Connect();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            database.Disconnect();
        }
    }
}
