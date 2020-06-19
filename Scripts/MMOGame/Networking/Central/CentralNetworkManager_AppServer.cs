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
            return ClientSendRequest(MMOMessageTypes.RequestAppServerRegister, message, callback);
        }

        public uint RequestAppServerAddress(CentralServerPeerType peerType, string extra, AckMessageCallback callback)
        {
            RequestAppServerAddressMessage message = new RequestAppServerAddressMessage();
            message.peerType = peerType;
            message.extra = extra;
            return ClientSendRequest(MMOMessageTypes.RequestAppServerAddress, message, callback);
        }

        protected void HandleRequestAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestAppServerRegisterMessage message = messageHandler.ReadMessage<RequestAppServerRegisterMessage>();
            ResponseAppServerRegisterMessage.Error error = ResponseAppServerRegisterMessage.Error.None;
            if (message.ValidateHash())
            {
                ResponseAppServerAddressMessage appServerAddressMessage;
                CentralServerPeerInfo peerInfo = message.peerInfo;
                peerInfo.connectionId = connectionId;
                switch (message.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[connectionId] = peerInfo;
                        if (LogInfo)
                            Logging.Log(LogTag, "Register Map Spawn Server: [" + connectionId + "]");
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
                                Logging.Log(LogTag, "Register Map Server: [" + connectionId + "] [" + sceneName + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.MapAlreadyExisted;
                            if (LogInfo)
                                Logging.Log(LogTag, "Register Map Server Failed: [" + connectionId + "] [" + sceneName + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.InstanceMapServer:
                        string instanceId = peerInfo.extra;
                        if (!instanceMapServerPeersByInstanceId.ContainsKey(instanceId))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            instanceMapServerPeersByInstanceId[instanceId] = peerInfo;
                            instanceMapServerPeers[connectionId] = peerInfo;
                            mapUserIds[connectionId] = new HashSet<string>();
                            if (LogInfo)
                                Logging.Log(LogTag, "Register Instance Map Server: [" + connectionId + "] [" + instanceId + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.EventAlreadyExisted;
                            if (LogInfo)
                                Logging.Log(LogTag, "Register Instance Map Server Failed: [" + connectionId + "] [" + instanceId + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        chatServerPeers[connectionId] = peerInfo;
                        // Send chat peer info to map servers
                        foreach (CentralServerPeerInfo mapServerPeer in mapServerPeers.Values)
                        {
                            appServerAddressMessage = new ResponseAppServerAddressMessage();
                            appServerAddressMessage.responseCode = AckResponseCode.Success;
                            appServerAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                            appServerAddressMessage.peerInfo = peerInfo;
                            ServerSendResponse(mapServerPeer.connectionId, MMOMessageTypes.ResponseAppServerAddress, appServerAddressMessage);
                        }
                        if (LogInfo)
                            Logging.Log(LogTag, "Register Chat Server: [" + connectionId + "]");
                        break;
                }
            }
            else
            {
                error = ResponseAppServerRegisterMessage.Error.InvalidHash;
                if (LogInfo)
                    Logging.Log(LogTag, "Register Server Failed: [" + connectionId + "] [" + error + "]");
            }

            ResponseAppServerRegisterMessage responseMessage = new ResponseAppServerRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseAppServerRegister, responseMessage);
        }

        /// <summary>
        /// This function will be used to send connection information to connected map servers and chat servers
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="peerInfo"></param>
        protected void BroadcastAppServers(long connectionId, CentralServerPeerInfo peerInfo)
        {
            ResponseAppServerAddressMessage appServerAddressMessage;
            // Send map peer info to other map server
            foreach (CentralServerPeerInfo mapServerPeer in mapServerPeers.Values)
            {
                // Send other info to current peer
                appServerAddressMessage = new ResponseAppServerAddressMessage();
                appServerAddressMessage.responseCode = AckResponseCode.Success;
                appServerAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                appServerAddressMessage.peerInfo = mapServerPeer;
                ServerSendResponse(connectionId, MMOMessageTypes.ResponseAppServerAddress, appServerAddressMessage);
                // Send current info to other peer
                appServerAddressMessage = new ResponseAppServerAddressMessage();
                appServerAddressMessage.responseCode = AckResponseCode.Success;
                appServerAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                appServerAddressMessage.peerInfo = peerInfo;
                ServerSendResponse(mapServerPeer.connectionId, MMOMessageTypes.ResponseAppServerAddress, appServerAddressMessage);
            }
            // Send chat peer info to new map server
            if (chatServerPeers.Count > 0)
            {
                CentralServerPeerInfo chatPeerInfo = chatServerPeers.Values.First();
                appServerAddressMessage = new ResponseAppServerAddressMessage();
                appServerAddressMessage.responseCode = AckResponseCode.Success;
                appServerAddressMessage.error = ResponseAppServerAddressMessage.Error.None;
                appServerAddressMessage.peerInfo = chatPeerInfo;
                ServerSendResponse(connectionId, MMOMessageTypes.ResponseAppServerAddress, appServerAddressMessage);
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
                            Logging.Log(LogTag, "Request Map Spawn Address: [" + connectionId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        if (LogInfo)
                            Logging.Log(LogTag, "Request Map Spawn Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    string mapName = message.extra;
                    if (!mapServerPeersBySceneName.TryGetValue(mapName, out peerInfo))
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        if (LogInfo)
                            Logging.Log(LogTag, "Request Map Address: [" + connectionId + "] [" + mapName + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.Chat:
                    if (chatServerPeers.Count > 0)
                    {
                        peerInfo = chatServerPeers.Values.First();
                        if (LogInfo)
                            Logging.Log(LogTag, "Request Chat Address: [" + connectionId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        if (LogInfo)
                            Logging.Log(LogTag, "Request Chat Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
            }
            ResponseAppServerAddressMessage responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerAddressMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseAppServerAddress, responseMessage);
        }

        protected void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseAppServerRegisterMessage message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseAppServerAddressMessage message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
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
                        if (!mapUserIds[connectionId].Contains(message.data.userId))
                        {
                            mapUserIds[connectionId].Add(message.data.userId);
                            if (LogInfo)
                                Logging.Log(LogTag, "Add map user: " + message.data.userId + " by " + connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Remove:
                        mapUserIds[connectionId].Remove(message.data.userId);
                        if (LogInfo)
                            Logging.Log(LogTag, "Remove map user: " + message.data.userId + " by " + connectionId);
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
