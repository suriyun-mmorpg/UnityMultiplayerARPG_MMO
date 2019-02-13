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
            RequestAppServerRegisterMessage message = new RequestAppServerRegisterMessage();
            message.peerInfo = peerInfo;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestAppServerRegister, message, callback);
        }

        public uint RequestAppServerAddress(CentralServerPeerType peerType, string extra, AckMessageCallback callback)
        {
            RequestAppServerAddressMessage message = new RequestAppServerAddressMessage();
            message.peerType = peerType;
            message.extra = extra;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestAppServerAddress, message, callback);
        }

        protected void HandleRequestAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestAppServerRegisterMessage message = messageHandler.ReadMessage<RequestAppServerRegisterMessage>();
            ResponseAppServerRegisterMessage.Error error = ResponseAppServerRegisterMessage.Error.None;
            if (message.ValidateHash())
            {
                CentralServerPeerInfo peerInfo = message.peerInfo;
                peerInfo.connectionId = connectionId;
                switch (message.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[connectionId] = peerInfo;
                        if (LogInfo)
                            Debug.Log("[Central] Register Map Spawn Server: [" + connectionId + "]");
                        break;
                    case CentralServerPeerType.MapServer:
                        string sceneName = peerInfo.extra;
                        if (!mapServerPeersBySceneName.ContainsKey(sceneName))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            mapServerPeersBySceneName[sceneName] = peerInfo;
                            mapServerPeers[connectionId] = peerInfo;
                            mapUserIds[connectionId] = new HashSet<string>();
                            if (LogInfo)
                                Debug.Log("[Central] Register Map Server: [" + connectionId + "] [" + sceneName + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.MapAlreadyExisted;
                            if (LogInfo)
                                Debug.Log("[Central] Register Map Server Failed: [" + connectionId + "] [" + sceneName + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.InstanceMapServer:
                        string eventId = peerInfo.extra;
                        if (!instanceMapServerPeersByInstanceId.ContainsKey(eventId))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            instanceMapServerPeersByInstanceId[eventId] = peerInfo;
                            instanceMapServerPeers[connectionId] = peerInfo;
                            mapUserIds[connectionId] = new HashSet<string>();
                            if (LogInfo)
                                Debug.Log("[Central] Register Instance Map Server: [" + connectionId + "] [" + eventId + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.EventAlreadyExisted;
                            if (LogInfo)
                                Debug.Log("[Central] Register Instance Map Server Failed: [" + connectionId + "] [" + eventId + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        chatServerPeers[connectionId] = peerInfo;
                        // Send chat peer info to map servers
                        foreach (CentralServerPeerInfo mapServerPeer in mapServerPeers.Values)
                        {
                            ResponseAppServerAddressMessage responseChatAddressMessage = new ResponseAppServerAddressMessage();
                            responseChatAddressMessage.responseCode = AckResponseCode.Success;
                            responseChatAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                            responseChatAddressMessage.peerInfo = peerInfo;
                            ServerSendPacket(mapServerPeer.connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseChatAddressMessage);
                        }
                        if (LogInfo)
                            Debug.Log("[Central] Register Chat Server: [" + connectionId + "]");
                        break;
                }
            }
            else
            {
                error = ResponseAppServerRegisterMessage.Error.InvalidHash;
                if (LogInfo)
                    Debug.Log("[Central] Register Server Failed: [" + connectionId + "] [" + error + "]");
            }

            ResponseAppServerRegisterMessage responseMessage = new ResponseAppServerRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerRegister, responseMessage);
        }

        /// <summary>
        /// This function will be used to send connection information to connected map servers and chat servers
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="peerInfo"></param>
        protected void BroadcastAppServers(long connectionId, CentralServerPeerInfo peerInfo)
        {
            // Send map peer info to other map server
            foreach (CentralServerPeerInfo mapServerPeer in mapServerPeers.Values)
            {
                // Send other info to current peer
                ResponseAppServerAddressMessage responseMapAddressMessage = new ResponseAppServerAddressMessage();
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
                CentralServerPeerInfo chatPeerInfo = chatServerPeers.Values.First();
                ResponseAppServerAddressMessage responseChatAddressMessage = new ResponseAppServerAddressMessage();
                responseChatAddressMessage.responseCode = AckResponseCode.Success;
                responseChatAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                responseChatAddressMessage.peerInfo = chatPeerInfo;
                ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseChatAddressMessage);
            }
        }

        protected void HandleRequestAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestAppServerAddressMessage message = messageHandler.ReadMessage<RequestAppServerAddressMessage>();
            ResponseAppServerAddressMessage.Error error = ResponseAppServerAddressMessage.Error.None;
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            switch (message.peerType)
            {
                // TODO: Balancing servers when there are multiple servers with same type
                case CentralServerPeerType.MapSpawnServer:
                    if (mapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = mapSpawnServerPeers.Values.First();
                        if (LogInfo)
                            Debug.Log("[Central] Request Map Spawn Address: [" + connectionId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        if (LogInfo)
                            Debug.Log("[Central] Request Map Spawn Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    string mapName = message.extra;
                    if (!mapServerPeersBySceneName.TryGetValue(mapName, out peerInfo))
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        if (LogInfo)
                            Debug.Log("[Central] Request Map Address: [" + connectionId + "] [" + mapName + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.Chat:
                    if (chatServerPeers.Count > 0)
                    {
                        peerInfo = chatServerPeers.Values.First();
                        if (LogInfo)
                            Debug.Log("[Central] Request Chat Address: [" + connectionId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        if (LogInfo)
                            Debug.Log("[Central] Request Chat Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
            }
            ResponseAppServerAddressMessage responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerAddressMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseAppServerAddress, responseMessage);
        }

        protected void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseAppServerRegisterMessage message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseAppServerAddressMessage message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected void HandleUpdateMapUser(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            if (mapUserIds.ContainsKey(connectionId))
            {
                switch (message.type)
                {
                    case UpdateUserCharacterMessage.UpdateType.Add:
                        if (!mapUserIds[connectionId].Contains(message.UserId))
                        {
                            mapUserIds[connectionId].Add(message.UserId);
                            if (LogInfo)
                                Debug.Log("[Central] Add map user: " + message.UserId + " by " + connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Remove:
                        mapUserIds[connectionId].Remove(message.UserId);
                        if (LogInfo)
                            Debug.Log("[Central] Remove map user: " + message.UserId + " by " + connectionId);
                        break;
                }
            }
        }

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, int time)
        {
            // TODO: Add salt
            MD5 algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
