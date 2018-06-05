using System.Collections.Generic;
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
        }

        public readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersByMapName = new Dictionary<string, CentralServerPeerInfo>();

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
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterServerMessage(CentralMsgTypes.ResponseAppServerRegister, HandleResponseAppServerRegister);
            RegisterServerMessage(CentralMsgTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
            RegisterServerMessage(CentralMsgTypes.ResponseUserLogin, HandleResponseUserLogin);
            RegisterServerMessage(CentralMsgTypes.ResponseUserRegister, HandleResponseUserRegister);
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
