using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class ClusterServer : LiteNetLibServer
    {
        public override string LogTag { get { return nameof(ClusterServer); } }

        private readonly CentralNetworkManager centralNetworkManager;
#if UNITY_STANDALONE && !CLIENT_BUILD
        internal Dictionary<string, SocialCharacterData> MapUsersByCharacterId { get; private set; } = new Dictionary<string, SocialCharacterData>();
        internal Dictionary<string, long> ConnectionIdsByCharacterId { get; private set; } = new Dictionary<string, long>();
        internal Dictionary<string, long> ConnectionIdsByCharacterName { get; private set; } = new Dictionary<string, long>();
        // Map spawn server peers
        internal Dictionary<long, CentralServerPeerInfo> MapSpawnServerPeers { get; private set; } = new Dictionary<long, CentralServerPeerInfo>();
        // Map server peers
        internal Dictionary<long, CentralServerPeerInfo> MapServerPeers { get; private set; } = new Dictionary<long, CentralServerPeerInfo>();
        internal Dictionary<string, CentralServerPeerInfo> MapServerPeersByMapId { get; private set; } = new Dictionary<string, CentralServerPeerInfo>();
        internal Dictionary<string, CentralServerPeerInfo> MapServerPeersByInstanceId { get; private set; } = new Dictionary<string, CentralServerPeerInfo>();
        // <Request Id, Response Handler> dictionary
        internal Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>> RequestSpawnMapHandlers = new Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>>();
#endif

        public ClusterServer(CentralNetworkManager centralNetworkManager) : base(new TcpTransport())
        {
            this.centralNetworkManager = centralNetworkManager;
#if UNITY_STANDALONE && !CLIENT_BUILD
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Generic
            RegisterRequestHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, HandleRequestAppServerRegister);
            RegisterRequestHandler<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress, HandleRequestAppServerAddress);
            // Map
            RegisterMessageHandler(MMOMessageTypes.Chat, HandleChat);
            RegisterMessageHandler(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            RegisterMessageHandler(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMember);
            RegisterMessageHandler(MMOMessageTypes.UpdateParty, HandleUpdateParty);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMember);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuild, HandleUpdateGuild);
            // Map-spawn
            RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleRequestSpawnMap);
            RegisterResponseHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleResponseSpawnMap);
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public bool StartServer()
        {
            return StartServer(centralNetworkManager.clusterServerPort, int.MaxValue);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected override void OnStopServer()
        {
            base.OnStopServer();
            MapUsersByCharacterId.Clear();
            ConnectionIdsByCharacterId.Clear();
            ConnectionIdsByCharacterName.Clear();
            MapSpawnServerPeers.Clear();
            MapServerPeers.Clear();
            MapServerPeersByMapId.Clear();
            MapServerPeersByInstanceId.Clear();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void OnServerReceive(TransportEventData eventData)
        {
            CentralServerPeerInfo tempPeerInfo;
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, "OnPeerConnected peer.ConnectionId: " + eventData.connectionId);
                    ConnectionIds.Add(eventData.connectionId);
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, "OnPeerDisconnected peer.ConnectionId: " + eventData.connectionId + " disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    ConnectionIds.Remove(eventData.connectionId);
                    // Remove disconnect map spawn server
                    MapSpawnServerPeers.Remove(eventData.connectionId);
                    // Remove disconnect map server
                    if (MapServerPeers.TryGetValue(eventData.connectionId, out tempPeerInfo))
                    {
                        MapServerPeersByMapId.Remove(tempPeerInfo.extra);
                        MapServerPeersByInstanceId.Remove(tempPeerInfo.extra);
                        MapServerPeers.Remove(eventData.connectionId);
                        RemoveMapUsers(eventData.connectionId);
                    }
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnPeerNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError + " errorMessage " + eventData.errorMessage);
                    break;
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void RemoveMapUsers(long connectionId)
        {
            List<KeyValuePair<string, SocialCharacterData>> mapUsers = MapUsersByCharacterId.ToList();
            foreach (KeyValuePair<string, SocialCharacterData> entry in mapUsers)
            {
                // Find characters which connected to disconnecting map server
                long tempConnectionId;
                if (!ConnectionIdsByCharacterId.TryGetValue(entry.Key, out tempConnectionId) || connectionId != tempConnectionId)
                    continue;

                // Send remove messages to other map servers
                UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, entry.Value, connectionId);

                // Clear disconnecting map users data
                MapUsersByCharacterId.Remove(entry.Key);
                ConnectionIdsByCharacterId.Remove(entry.Key);
                ConnectionIdsByCharacterName.Remove(entry.Value.characterName);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid HandleRequestAppServerRegister(
            RequestHandlerData requestHandler,
            RequestAppServerRegisterMessage request,
            RequestProceedResultDelegate<ResponseAppServerRegisterMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            if (request.ValidateHash())
            {
                CentralServerPeerInfo peerInfo = request.peerInfo;
                peerInfo.connectionId = connectionId;
                switch (request.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        MapSpawnServerPeers[connectionId] = peerInfo;
                        Logging.Log(LogTag, "Register Map Spawn Server: [" + connectionId + "]");
                        break;
                    case CentralServerPeerType.MapServer:
                        // Extra is map ID
                        if (!MapServerPeersByMapId.ContainsKey(peerInfo.extra))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            MapServerPeersByMapId[peerInfo.extra] = peerInfo;
                            MapServerPeers[connectionId] = peerInfo;
                            Logging.Log(LogTag, "Register Map Server: [" + connectionId + "] [" + peerInfo.extra + "]");
                        }
                        else
                        {
                            message = UITextKeys.UI_ERROR_MAP_EXISTED;
                            Logging.Log(LogTag, "Register Map Server Failed: [" + connectionId + "] [" + peerInfo.extra + "] [" + message + "]");
                        }
                        break;
                    case CentralServerPeerType.InstanceMapServer:
                        // Extra is instance ID
                        if (!MapServerPeersByInstanceId.ContainsKey(peerInfo.extra))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            MapServerPeersByInstanceId[peerInfo.extra] = peerInfo;
                            MapServerPeers[connectionId] = peerInfo;
                            Logging.Log(LogTag, "Register Instance Map Server: [" + connectionId + "] [" + peerInfo.extra + "]");
                        }
                        else
                        {
                            message = UITextKeys.UI_ERROR_EVENT_EXISTED;
                            Logging.Log(LogTag, "Register Instance Map Server Failed: [" + connectionId + "] [" + peerInfo.extra + "] [" + message + "]");
                        }
                        break;
                }
            }
            else
            {
                message = UITextKeys.UI_ERROR_INVALID_SERVER_HASH;
                Logging.Log(LogTag, "Register Server Failed: [" + connectionId + "] [" + message + "]");
            }
            // Response
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseAppServerRegisterMessage()
                {
                    message = message,
                });
            await UniTask.Yield();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        /// <summary>
        /// This function will be used to send connection information to connected map servers and cluster servers
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="broadcastPeerInfo"></param>
        private void BroadcastAppServers(long connectionId, CentralServerPeerInfo broadcastPeerInfo)
        {
            // Send map peer info to other map server
            foreach (CentralServerPeerInfo mapPeerInfo in MapServerPeers.Values)
            {
                // Send other info to current peer
                SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, (writer) => writer.PutValue(new ResponseAppServerAddressMessage()
                {
                    message = UITextKeys.NONE,
                    peerInfo = mapPeerInfo,
                }));
                // Send current info to other peer
                SendPacket(mapPeerInfo.connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, (writer) => writer.PutValue(new ResponseAppServerAddressMessage()
                {
                    message = UITextKeys.NONE,
                    peerInfo = broadcastPeerInfo,
                }));
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid HandleRequestAppServerAddress(
            RequestHandlerData requestHandler,
            RequestAppServerAddressMessage request,
            RequestProceedResultDelegate<ResponseAppServerAddressMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            switch (request.peerType)
            {
                // TODO: Balancing servers when there are multiple servers with same type
                case CentralServerPeerType.MapSpawnServer:
                    if (MapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = MapSpawnServerPeers.Values.First();
                        Logging.Log(LogTag, "Request Map Spawn Address: [" + connectionId + "]");
                    }
                    else
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, "Request Map Spawn Address: [" + connectionId + "] [" + message + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    string mapName = request.extra;
                    if (!MapServerPeersByMapId.TryGetValue(mapName, out peerInfo))
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, "Request Map Address: [" + connectionId + "] [" + mapName + "] [" + message + "]");
                    }
                    break;
                case CentralServerPeerType.InstanceMapServer:
                    string instanceId = request.extra;
                    if (!MapServerPeersByInstanceId.TryGetValue(instanceId, out peerInfo))
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, "Request Map Address: [" + connectionId + "] [" + instanceId + "] [" + message + "]");
                    }
                    break;
            }
            // Response
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseAppServerAddressMessage()
                {
                    message = message,
                    peerInfo = peerInfo,
                });
            await UniTask.Yield();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleChat(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            switch (message.channel)
            {
                case ChatChannel.Local:
                case ChatChannel.Global:
                case ChatChannel.System:
                case ChatChannel.Party:
                case ChatChannel.Guild:
                    // Send message to all map servers, let's map servers filter messages
                    SendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    break;
                case ChatChannel.Whisper:
                    long senderConnectionId = 0;
                    long receiverConnectionId = 0;
                    // Send message to map server which have the character
                    if (!string.IsNullOrEmpty(message.sender) && ConnectionIdsByCharacterName.TryGetValue(message.sender, out senderConnectionId))
                        SendPacket(senderConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    if (!string.IsNullOrEmpty(message.receiver) && ConnectionIdsByCharacterName.TryGetValue(message.receiver, out receiverConnectionId) && (receiverConnectionId != senderConnectionId))
                        SendPacket(receiverConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    break;
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleUpdateMapUser(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            SocialCharacterData userData;
            switch (message.type)
            {
                case UpdateUserCharacterMessage.UpdateType.Add:
                    if (!MapUsersByCharacterId.ContainsKey(message.character.id))
                    {
                        MapUsersByCharacterId[message.character.id] = message.character;
                        ConnectionIdsByCharacterId[message.character.id] = connectionId;
                        ConnectionIdsByCharacterName[message.character.characterName] = connectionId;
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Add, message.character, connectionId);
                    }
                    break;
                case UpdateUserCharacterMessage.UpdateType.Remove:
                    if (MapUsersByCharacterId.TryGetValue(message.character.id, out userData))
                    {
                        MapUsersByCharacterId.Remove(userData.id);
                        ConnectionIdsByCharacterId.Remove(userData.id);
                        ConnectionIdsByCharacterName.Remove(userData.characterName);
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, userData, connectionId);
                    }
                    break;
                case UpdateUserCharacterMessage.UpdateType.Online:
                    if (MapUsersByCharacterId.ContainsKey(message.character.id))
                    {
                        MapUsersByCharacterId[message.character.id] = message.character;
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Online, message.character, connectionId);
                    }
                    break;
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleUpdatePartyMember(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (MapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in MapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdatePartyMember, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleUpdateParty(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (MapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in MapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateParty, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleUpdateGuildMember(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (MapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in MapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuildMember, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleUpdateGuild(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (MapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in MapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuild, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void UpdateMapUser(UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData, long exceptConnectionId)
        {
            foreach (long mapServerConnectionId in MapServerPeers.Keys)
            {
                if (mapServerConnectionId == exceptConnectionId)
                    continue;

                UpdateMapUser(mapServerConnectionId, updateType, userData);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void UpdateMapUser(long connectionId, UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData)
        {
            UpdateUserCharacterMessage message = new UpdateUserCharacterMessage();
            message.type = updateType;
            message.character = userData;
            SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, (writer) => writer.PutValue(message));
        }
#endif

        public bool MapContainsUser(string userId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            foreach (SocialCharacterData mapUser in MapUsersByCharacterId.Values)
            {
                if (mapUser.userId.Equals(userId))
                    return true;
            }
#endif
            return false;
        }

        public bool RequestSpawnMap(long connectionId, string sceneName, string instanceId, Vector3 instanceWarpPosition, bool instanceWarpOverrideRotation, Vector3 instanceWarpRotation)
        {
            return RequestSpawnMap(connectionId, new RequestSpawnMapMessage()
            {
                mapId = sceneName,
                instanceId = instanceId,
                instanceWarpPosition = instanceWarpPosition,
                instanceWarpOverrideRotation = instanceWarpOverrideRotation,
                instanceWarpRotation = instanceWarpRotation,
            });
        }

        public bool RequestSpawnMap(long connectionId, RequestSpawnMapMessage message)
        {
            return SendRequest(connectionId, MMORequestTypes.RequestSpawnMap, message, millisecondsTimeout: centralNetworkManager.mapSpawnMillisecondsTimeout);
        }

        /// <summary>
        /// This is function which read request from map server to spawn another map servers
        /// Then it will response back when requested map server is ready
        /// </summary>
        /// <param name="messageHandler"></param>
        protected UniTaskVoid HandleRequestSpawnMap(
            RequestHandlerData requestHandler,
            RequestSpawnMapMessage request,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            string requestId = GenericUtils.GetUniqueId();
            request.requestId = requestId;
            List<long> connectionIds = new List<long>(MapSpawnServerPeers.Keys);
            // Random map-spawn server to spawn map, will use returning ackId as reference to map-server's transport handler and ackId
            RequestSpawnMap(connectionIds[Random.Range(0, connectionIds.Count)], request);
            // Add ack Id / transport handler to dictionary which will be used in OnRequestSpawnMap() function 
            // To send map spawn response to map-server
            RequestSpawnMapHandlers.Add(requestId, result);
#endif
            return default;
        }

        protected void HandleResponseSpawnMap(
            ResponseHandlerData requestHandler,
            AckResponseCode responseCode,
            ResponseSpawnMapMessage response)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Forward responses to map server transport handler
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result;
            if (RequestSpawnMapHandlers.TryGetValue(response.requestId, out result))
                result.Invoke(responseCode, response);
#endif
        }

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, long time)
        {
            MD5 algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
