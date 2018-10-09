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
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestAppServerRegister, message, callback);
        }

        public uint RequestAppServerAddress(CentralServerPeerType peerType, string extra, AckMessageCallback callback)
        {
            var message = new RequestAppServerAddressMessage();
            message.peerType = peerType;
            message.extra = extra;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestAppServerAddress, message, callback);
        }

        protected void HandleRequestAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestAppServerRegisterMessage>();
            var error = ResponseAppServerRegisterMessage.Error.None;
            if (message.ValidateHash())
            {
                var peerInfo = message.peerInfo;
                peerInfo.connectionId = connectionId;
                switch (message.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[connectionId] = peerInfo;
                        Debug.Log("[Central] Register Map Spawn Server: [" + connectionId + "]");
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
                                ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseMapAddressMessage);
                                // Send current info to other peer
                                responseMapAddressMessage = new ResponseAppServerAddressMessage();
                                responseMapAddressMessage.responseCode = AckResponseCode.Success;
                                responseMapAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                                responseMapAddressMessage.peerInfo = peerInfo;
                                ServerSendPacket(mapServerPeer.connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseMapAddressMessage);
                            }
                            // Send chat peer info to new map server
                            if (chatServerPeers.Count > 0)
                            {
                                var chatPeerInfo = chatServerPeers.Values.First();
                                var responseChatAddressMessage = new ResponseAppServerAddressMessage();
                                responseChatAddressMessage.responseCode = AckResponseCode.Success;
                                responseChatAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                                responseChatAddressMessage.peerInfo = chatPeerInfo;
                                ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseChatAddressMessage);
                            }
                            // Collects server data
                            mapServerPeersBySceneName[sceneName] = peerInfo;
                            mapServerPeers[connectionId] = peerInfo;
                            mapUserIds[connectionId] = new HashSet<string>();
                            Debug.Log("[Central] Register Map Server: [" + connectionId + "] [" + sceneName + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.MapAlreadyExisted;
                            Debug.Log("[Central] Register Map Server Failed: [" + connectionId + "] [" + sceneName + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        chatServerPeers[connectionId] = peerInfo;
                        // Send chat peer info to map servers
                        foreach (var mapServerPeer in mapServerPeers.Values)
                        {
                            var responseChatAddressMessage = new ResponseAppServerAddressMessage();
                            responseChatAddressMessage.responseCode = AckResponseCode.Success;
                            responseChatAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                            responseChatAddressMessage.peerInfo = peerInfo;
                            ServerSendPacket(mapServerPeer.connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseChatAddressMessage);
                        }
                        Debug.Log("[Central] Register Chat Server: [" + connectionId + "]");
                        break;
                }
            }
            else
            {
                error = ResponseAppServerRegisterMessage.Error.InvalidHash;
                Debug.Log("[Central] Register Server Failed: [" + connectionId + "] [" + error + "]");
            }

            var responseMessage = new ResponseAppServerRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerRegister, responseMessage);
        }

        protected void HandleRequestAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
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
                        Debug.Log("[Central] Request Map Spawn Address: [" + connectionId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Map Spawn Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    var mapName = message.extra;
                    if (!mapServerPeersBySceneName.TryGetValue(mapName, out peerInfo))
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Map Address: [" + connectionId + "] [" + mapName + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.Chat:
                    if (chatServerPeers.Count > 0)
                    {
                        peerInfo = chatServerPeers.Values.First();
                        Debug.Log("[Central] Request Chat Address: [" + connectionId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Chat Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
            }
            var responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerAddressMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseMessage);
        }

        protected void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleUpdateMapUser(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            if (mapUserIds.ContainsKey(connectionId))
            {
                switch (message.type)
                {
                    case UpdateUserCharacterMessage.UpdateType.Add:
                        if (!mapUserIds[connectionId].Contains(message.UserId))
                        {
                            mapUserIds[connectionId].Add(message.UserId);
                            Debug.Log("[Central] Add map user: " + message.UserId + " by " + connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Remove:
                        mapUserIds[connectionId].Remove(message.UserId);
                        Debug.Log("[Central] Remove map user: " + message.UserId + " by " + connectionId);
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
