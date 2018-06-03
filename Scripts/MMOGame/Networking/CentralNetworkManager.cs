using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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

        public delegate void ResponseAppServerRegistrationCallback(ResponseAppServerRegistrationMessage response);
        public delegate void ResponseAppServerAddressCallback(ResponseAppServerAddressMessage response);

        public readonly Dictionary<long, CentralServerPeerInfo> loginServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> chatServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<long, CentralServerPeerInfo> mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public readonly Dictionary<string, CentralServerPeerInfo> mapServerPeersByMapName = new Dictionary<string, CentralServerPeerInfo>();

        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;

        private readonly Dictionary<uint, ResponseAppServerRegistrationCallback> appServerRegistrationAcks = new Dictionary<uint, ResponseAppServerRegistrationCallback>();
        private readonly Dictionary<uint, ResponseAppServerAddressCallback> appServerAddressAcks = new Dictionary<uint, ResponseAppServerAddressCallback>();
        private uint nextAppServerRegistrationAckId = 1;
        private uint nextAppServerAddressAckId = 1;
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

        public uint RequestAppServerRegistration(CentralServerPeerInfo peerInfo, ResponseAppServerRegistrationCallback responseCallback)
        {
            var message = new RequestAppServerRegistrationMessage();
            var ackId = nextAppServerRegistrationAckId++;
            lock (appServerRegistrationAcks)
                appServerRegistrationAcks.Add(ackId, responseCallback);
            message.ackId = ackId;
            message.peerInfo = peerInfo;
            SendPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestAppServerRegistration, message);
            return ackId;
        }

        public uint RequestAppServerAddress(CentralServerPeerType peerType, string extra, ResponseAppServerAddressCallback responseCallback)
        {
            var message = new RequestAppServerAddressMessage();
            var ackId = nextAppServerAddressAckId++;
            lock (appServerAddressAcks)
                appServerAddressAcks.Add(ackId, responseCallback);
            message.ackId = ackId;
            message.peerType = peerType;
            message.extra = extra;
            SendPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestAppServerAddress, message);
            return ackId;
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
                        Debug.Log("[Central] Register Login Server: [" + peer.ConnectId + "]");
                        break;
                    case CentralServerPeerType.ChatServer:
                        chatServerPeers[peer.ConnectId] = peerInfo;
                        Debug.Log("[Central] Register Chat Server: [" + peer.ConnectId + "]");
                        break;
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[peer.ConnectId] = peerInfo;
                        Debug.Log("[Central] Register Map Spawn Server: [" + peer.ConnectId + "]");
                        break;
                    case CentralServerPeerType.MapServer:
                        var mapName = peerInfo.extra;
                        if (!mapServerPeersByMapName.ContainsKey(mapName))
                        {
                            mapServerPeersByMapName[mapName] = peerInfo;
                            mapServerPeers[peer.ConnectId] = peerInfo;
                            Debug.Log("[Central] Register Map Server: [" + peer.ConnectId + "] [" + mapName + "]");
                        }
                        else
                        {
                            error = "MAP_ALREADY_EXISTED";
                            Debug.Log("[Central] Register Map Server Failed: [" + peer.ConnectId + "] [" + mapName + "] [" + error + "]");
                        }
                        break;
                }
            }
            else
            {
                error = "INVALID_HASH";
                Debug.Log("[Central] Register Server Failed: [" + peer.ConnectId + "] [" + error + "]");
            }

            var responseMessage = new ResponseAppServerRegistrationMessage();
            responseMessage.ackId = message.ackId;
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
                    {
                        peerInfo = loginServerPeers.Values.First();
                        Debug.Log("[Central] Request Login Address: [" + peer.ConnectId + "]");
                    }
                    else
                    {
                        error = "SERVER_NOT_FOUND";
                        Debug.Log("[Central] Request Login Address: [" + peer.ConnectId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.ChatServer:
                    if (chatServerPeers.Count > 0)
                    {
                        peerInfo = chatServerPeers.Values.First();
                        Debug.Log("[Central] Request Chat Address: [" + peer.ConnectId + "]");
                    }
                    else
                    {
                        error = "SERVER_NOT_FOUND";
                        Debug.Log("[Central] Request Chat Address: [" + peer.ConnectId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapSpawnServer:
                    if (mapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = mapSpawnServerPeers.Values.First();
                        Debug.Log("[Central] Request Map Spawn Address: [" + peer.ConnectId + "]");
                    }
                    else
                    {
                        error = "SERVER_NOT_FOUND";
                        Debug.Log("[Central] Request Map Spawn Address: [" + peer.ConnectId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    var mapName = message.extra;
                    if (!mapServerPeersByMapName.TryGetValue(mapName, out peerInfo))
                    {
                        error = "SERVER_NOT_FOUND";
                        Debug.Log("[Central] Request Map Address: [" + peer.ConnectId + "] [" + mapName + "] [" + error + "]");
                    }
                    break;
            }
            var responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseAppServerAddress, responseMessage);
        }

        protected virtual void HandleResponseAppServerRegistration(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerRegistrationMessage>();
            var ackId = message.ackId;
            lock (appServerRegistrationAcks)
            {
                if (appServerRegistrationAcks.ContainsKey(ackId))
                {
                    appServerRegistrationAcks[ackId](message);
                    appServerRegistrationAcks.Remove(ackId);
                }
            }
        }

        protected virtual void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            var ackId = message.ackId;
            lock (appServerAddressAcks)
            {
                if (appServerAddressAcks.ContainsKey(ackId))
                {
                    appServerAddressAcks[ackId](message);
                    appServerAddressAcks.Remove(ackId);
                }
            }
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
