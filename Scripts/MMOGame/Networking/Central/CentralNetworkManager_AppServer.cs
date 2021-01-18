using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public bool RequestAppServerRegister(CentralServerPeerInfo peerInfo)
        {
            return ClientSendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
            });
        }

        public bool RequestAppServerAddress(CentralServerPeerType peerType, string extra)
        {
            return ClientSendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerAddressMessage()
            {
                peerType = peerType,
                extra = extra,
            });
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected UniTaskVoid HandleRequestAppServerRegister(
            RequestHandlerData requestHandler,
            RequestAppServerRegisterMessage request,
            RequestProceedResultDelegate<ResponseAppServerRegisterMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys error = UITextKeys.NONE;
            if (request.ValidateHash())
            {
                ResponseAppServerAddressMessage responseAppServerAddressMessage;
                CentralServerPeerInfo peerInfo = request.peerInfo;
                peerInfo.connectionId = connectionId;
                switch (request.peerInfo.peerType)
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
                            error = UITextKeys.UI_ERROR_MAP_EXISTED;
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
                            error = UITextKeys.UI_ERROR_EVENT_EXISTED;
                            if (LogInfo)
                                Logging.Log(LogTag, "Register Instance Map Server Failed: [" + connectionId + "] [" + instanceId + "] [" + error + "]");
                        }
                        break;
                    case CentralServerPeerType.Chat:
                        chatServerPeers[connectionId] = peerInfo;
                        // Send chat peer info to map servers
                        responseAppServerAddressMessage = new ResponseAppServerAddressMessage()
                        {
                            error = UITextKeys.NONE,
                            peerInfo = peerInfo,
                        };
                        foreach (CentralServerPeerInfo mapServerPeer in mapServerPeers.Values)
                        {
                            ServerSendPacket(mapServerPeer.connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, responseAppServerAddressMessage);
                        }
                        if (LogInfo)
                            Logging.Log(LogTag, "Register Chat Server: [" + connectionId + "]");
                        break;
                }
            }
            else
            {
                error = UITextKeys.UI_ERROR_INVALID_SERVER_HASH;
                if (LogInfo)
                    Logging.Log(LogTag, "Register Server Failed: [" + connectionId + "] [" + error + "]");
            }
            // Response
            result.Invoke(
                error == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseAppServerRegisterMessage()
                {
                    error = error,
                });
            return default;
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        /// <summary>
        /// This function will be used to send connection information to connected map servers and chat servers
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="broadcastPeerInfo"></param>
        protected void BroadcastAppServers(long connectionId, CentralServerPeerInfo broadcastPeerInfo)
        {
            // Send map peer info to other map server
            foreach (CentralServerPeerInfo mapPeerInfo in mapServerPeers.Values)
            {
                // Send other info to current peer
                ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, new ResponseAppServerAddressMessage()
                {
                    error = UITextKeys.NONE,
                    peerInfo = mapPeerInfo,
                });
                // Send current info to other peer
                ServerSendPacket(mapPeerInfo.connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, new ResponseAppServerAddressMessage()
                {
                    error = UITextKeys.NONE,
                    peerInfo = broadcastPeerInfo,
                });
            }
            // Send chat peer info to new map server
            if (chatServerPeers.Count > 0)
            {
                CentralServerPeerInfo chatPeerInfo = chatServerPeers.Values.First();
                ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, new ResponseAppServerAddressMessage()
                {
                    error = UITextKeys.NONE,
                    peerInfo = chatPeerInfo,
                });
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected UniTaskVoid HandleRequestAppServerAddress(
            RequestHandlerData requestHandler,
            RequestAppServerAddressMessage request,
            RequestProceedResultDelegate<ResponseAppServerAddressMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys error = UITextKeys.NONE;
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            switch (request.peerType)
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
                        error = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        if (LogInfo)
                            Logging.Log(LogTag, "Request Map Spawn Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    string mapName = request.extra;
                    if (!mapServerPeersBySceneName.TryGetValue(mapName, out peerInfo))
                    {
                        error = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
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
                        error = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        if (LogInfo)
                            Logging.Log(LogTag, "Request Chat Address: [" + connectionId + "] [" + error + "]");
                    }
                    break;
            }
            // Response
            result.Invoke(
                error == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseAppServerAddressMessage()
                {
                    error = error,
                    peerInfo = peerInfo,
                });
            return default;
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected void HandleUpdateMapUser(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
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
#endif

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, int time)
        {
            MD5 algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
