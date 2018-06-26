using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestAppServerRegister(CentralServerPeerInfo peerInfo, AckMessageCallback callback)
        {
            var message = new RequestAppServerRegisterMessage();
            message.peerInfo = peerInfo;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestAppServerRegister, message, callback);
        }

        public uint RequestAppServerAddress(CentralServerPeerType peerType, string extra, AckMessageCallback callback)
        {
            var message = new RequestAppServerAddressMessage();
            message.peerType = peerType;
            message.extra = extra;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestAppServerAddress, message, callback);
        }

        protected void HandleRequestAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestAppServerRegisterMessage>();
            var error = ResponseAppServerRegisterMessage.Error.None;
            if (message.ValidateHash())
            {
                var peerInfo = message.peerInfo;
                peerInfo.peer = peer;
                switch (message.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[peer.ConnectId] = peerInfo;
                        SpawnPublicMaps(peer);
                        Debug.Log("[Central] Register Map Spawn Server: [" + peer.ConnectId + "]");
                        break;
                    case CentralServerPeerType.MapServer:
                        var sceneName = peerInfo.extra;
                        if (!mapServerPeersBySceneName.ContainsKey(sceneName))
                        {
                            // Send map peer info to other map server
                            foreach (var mapServerPeer in mapServerPeers.Values)
                            {
                                // Send other info to current peer
                                var responseMapAddressMessage = new ResponseAppServerAddressMessage();
                                responseMapAddressMessage.responseCode = AckResponseCode.Success;
                                responseMapAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                                responseMapAddressMessage.peerInfo = mapServerPeer;
                                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseAppServerAddress, responseMapAddressMessage);
                                // Send current info to other peer
                                responseMapAddressMessage = new ResponseAppServerAddressMessage();
                                responseMapAddressMessage.responseCode = AckResponseCode.Success;
                                responseMapAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                                responseMapAddressMessage.peerInfo = peerInfo;
                                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, mapServerPeer.peer, MMOMessageTypes.ResponseAppServerAddress, responseMapAddressMessage);
                            }
                            // Send chat peer info to new map server
                            if (chatServerPeers.Count > 0)
                            {
                                var chatPeerInfo = chatServerPeers.Values.First();
                                var responseChatAddressMessage = new ResponseAppServerAddressMessage();
                                responseChatAddressMessage.responseCode = AckResponseCode.Success;
                                responseChatAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                                responseChatAddressMessage.peerInfo = chatPeerInfo;
                                LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseAppServerAddress, responseChatAddressMessage);
                            }
                            // Collects server data
                            mapServerPeersBySceneName[sceneName] = peerInfo;
                            mapServerPeers[peer.ConnectId] = peerInfo;
                            mapUserIds[peer.ConnectId] = new HashSet<string>();
                            Debug.Log("[Central] Register Map Server: [" + peer.ConnectId + "] [" + sceneName + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.MapAlreadyExisted;
                            Debug.Log("[Central] Register Map Server Failed: [" + peer.ConnectId + "] [" + sceneName + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        chatServerPeers[peer.ConnectId] = peerInfo;
                        // Send chat peer info to map servers
                        foreach (var mapServerPeer in mapServerPeers.Values)
                        {
                            var responseChatAddressMessage = new ResponseAppServerAddressMessage();
                            responseChatAddressMessage.responseCode = AckResponseCode.Success;
                            responseChatAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                            responseChatAddressMessage.peerInfo = peerInfo;
                            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, mapServerPeer.peer, MMOMessageTypes.ResponseAppServerAddress, responseChatAddressMessage);
                        }
                        Debug.Log("[Central] Register Chat Server: [" + peer.ConnectId + "]");
                        break;
                }
            }
            else
            {
                error = ResponseAppServerRegisterMessage.Error.InvalidHash;
                Debug.Log("[Central] Register Server Failed: [" + peer.ConnectId + "] [" + error + "]");
            }

            var responseMessage = new ResponseAppServerRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseAppServerRegister, responseMessage);
        }

        protected void HandleRequestAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestAppServerAddressMessage>();
            var error = ResponseAppServerAddressMessage.Error.None;
            var peerInfo = new CentralServerPeerInfo();
            switch (message.peerType)
            {
                // TODO: Balancing servers when there are multiple servers with same type
                case CentralServerPeerType.MapSpawnServer:
                    if (mapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = mapSpawnServerPeers.Values.First();
                        Debug.Log("[Central] Request Map Spawn Address: [" + peer.ConnectId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Map Spawn Address: [" + peer.ConnectId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    var mapName = message.extra;
                    if (!mapServerPeersBySceneName.TryGetValue(mapName, out peerInfo))
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Map Address: [" + peer.ConnectId + "] [" + mapName + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.Chat:
                    if (chatServerPeers.Count > 0)
                    {
                        peerInfo = chatServerPeers.Values.First();
                        Debug.Log("[Central] Request Chat Address: [" + peer.ConnectId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Chat Address: [" + peer.ConnectId + "] [" + error + "]");
                    }
                    break;
            }
            var responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerAddressMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseAppServerAddress, responseMessage);
        }

        protected void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleUpdateMapUser(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdateMapUserMessage>();
            if (mapUserIds.ContainsKey(peer.ConnectId))
            {
                switch (message.type)
                {
                    case UpdateMapUserMessage.UpdateType.Add:
                        if (!mapUserIds[peer.ConnectId].Contains(message.userData.userId))
                        {
                            mapUserIds[peer.ConnectId].Add(message.userData.userId);
                            Debug.Log("[Central] Add map user: " + message.userData.userId + " by " + peer.ConnectId);
                        }
                        break;
                    case UpdateMapUserMessage.UpdateType.Remove:
                        mapUserIds[peer.ConnectId].Remove(message.userData.userId);
                        Debug.Log("[Central] Remove map user: " + message.userData.userId + " by " + peer.ConnectId);
                        break;
                }
            }
        }

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, int time)
        {
            // TODO: Add salt
            var algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
