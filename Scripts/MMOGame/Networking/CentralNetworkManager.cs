using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace Insthync.MMOG
{
    public class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public class CentralMsgTypes
        {
            public const short RequestAppServerRegistration = 0;
            public const short ResponseAppServerRegistration = 1;
            public const short RequestAppServerAddress = 2;
            public const short ResponseAppServerAddress = 3;
        }

        public readonly Dictionary<long, CentralServerPeerInfo> loginServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> chatServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersByMapName = new Dictionary<string, CentralServerPeerInfo>();

        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;
        public System.Action<ResponseAppServerRegistrationMessage> onAppServerRegistrationResponded;
        public System.Action<ResponseAppServerAddressMessage> onAppServerAddressResponded;
        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            RegisterServerMessage(CentralMsgTypes.RequestAppServerRegistration, HandleRequestAppServerRegistration);
            RegisterServerMessage(CentralMsgTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterServerMessage(CentralMsgTypes.ResponseAppServerRegistration, HandleResponseAppServerRegistration);
            RegisterServerMessage(CentralMsgTypes.ResponseAppServerAddress, HandleResponseAppServerAddress);
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(peer, disconnectInfo);
            loginServerPeers.Remove(peer.ConnectId);
            chatServerPeers.Remove(peer.ConnectId);
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

        public void RequestAppServerRegistration(CentralServerPeerInfo peerInfo)
        {
            var message = new RequestAppServerRegistrationMessage();
            message.peerInfo = peerInfo;
            SendPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestAppServerRegistration, message);
        }

        public void RequestAppServerAddress(CentralServerPeerType peerType, string extra)
        {
            var message = new RequestAppServerAddressMessage();
            message.peerType = peerType;
            message.extra = extra;
            SendPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestAppServerAddress, message);
        }

        #region Message Handlers
        protected virtual void HandleRequestAppServerRegistration(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestAppServerRegistrationMessage>();
            var error = string.Empty;
            if (message.ValidateHash())
            {
                var peerInfo = message.peerInfo;
                switch (message.peerInfo.peerType)
                {
                    case CentralServerPeerType.LoginServer:
                        loginServerPeers[peer.ConnectId] = peerInfo;
                        break;
                    case CentralServerPeerType.ChatServer:
                        chatServerPeers[peer.ConnectId] = peerInfo;
                        break;
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[peer.ConnectId] = peerInfo;
                        break;
                    case CentralServerPeerType.MapServer:
                        var mapName = peerInfo.extra;
                        if (!mapServerPeersByMapName.ContainsKey(mapName))
                        {
                            mapServerPeersByMapName[mapName] = peerInfo;
                            mapServerPeers[peer.ConnectId] = peerInfo;
                        }
                        else
                            error = "MAP_ALREADY_EXISTED";
                        break;
                }
            }
            else
                error = "INVALID_HASH";

            var responseMessage = new ResponseAppServerRegistrationMessage();
            responseMessage.error = error;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseAppServerRegistration, responseMessage);
        }

        protected virtual void HandleRequestAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestAppServerAddressMessage>();
            var error = string.Empty;
            var peerInfo = new CentralServerPeerInfo();
            switch (message.peerType)
            {
                // TODO: Balancing servers
                case CentralServerPeerType.LoginServer:
                    if (loginServerPeers.Count > 0)
                        peerInfo = loginServerPeers.Values.First();
                    else
                        error = "SERVER_NOT_FOUND";
                    break;
                case CentralServerPeerType.ChatServer:
                    if (chatServerPeers.Count > 0)
                        peerInfo = chatServerPeers.Values.First();
                    else
                        error = "SERVER_NOT_FOUND";
                    break;
                case CentralServerPeerType.MapSpawnServer:
                    if (mapSpawnServerPeers.Count > 0)
                        peerInfo = mapSpawnServerPeers.Values.First();
                    else
                        error = "SERVER_NOT_FOUND";
                    break;
                case CentralServerPeerType.MapServer:
                    var mapName = message.extra;
                    if (!mapServerPeersByMapName.TryGetValue(mapName, out peerInfo))
                        error = "SERVER_NOT_FOUND";
                    break;
            }
            var responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseAppServerAddress, responseMessage);
        }

        protected virtual void HandleResponseAppServerRegistration(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerRegistrationMessage>();
            if (onAppServerRegistrationResponded != null)
                onAppServerRegistrationResponded(message);
        }

        protected virtual void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            if (onAppServerAddressResponded != null)
                onAppServerAddressResponded(message);
        }
        #endregion

        public static string GetAppServerRegistrationHash(CentralServerPeerType peerType, int time)
        {
            // TODO: Add salt
            var algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
