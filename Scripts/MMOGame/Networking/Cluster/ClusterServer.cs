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

        private readonly CentralNetworkManager _centralNetworkManager;
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private Dictionary<string, SocialCharacterData> _mapUsersByCharacterId = new Dictionary<string, SocialCharacterData>();
        public IReadOnlyDictionary<string, SocialCharacterData> MapUsersByCharacterId => _mapUsersByCharacterId;


        private Dictionary<string, long> _connectionIdsByCharacterId = new Dictionary<string, long>();
        public IReadOnlyDictionary<string, long> ConnectionIdsByCharacterId => _connectionIdsByCharacterId;

        private Dictionary<string, long> _connectionIdsByCharacterName = new Dictionary<string, long>();
        public IReadOnlyDictionary<string, long> ConnectionIdsByCharacterName => _connectionIdsByCharacterName;

        // Map spawn server peers
        private Dictionary<long, CentralServerPeerInfo> _mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public IReadOnlyDictionary<long, CentralServerPeerInfo> MapSpawnServerPeers => _mapSpawnServerPeers;

        // Map server peers
        private Dictionary<long, CentralServerPeerInfo> _mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public IReadOnlyDictionary<long, CentralServerPeerInfo> MapServerPeers => _mapServerPeers;

        private Dictionary<string, CentralServerPeerInfo> _mapServerPeersByMapId = new Dictionary<string, CentralServerPeerInfo>();
        public IReadOnlyDictionary<string, CentralServerPeerInfo> MapServerPeersByMapId => _mapServerPeersByMapId;

        private Dictionary<string, CentralServerPeerInfo> _mapServerPeersByInstanceId = new Dictionary<string, CentralServerPeerInfo>();
        public IReadOnlyDictionary<string, CentralServerPeerInfo> MapServerPeersByInstanceId => _mapServerPeersByInstanceId;

        // <Request Id, Response Handler> dictionary
        private Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>> _requestSpawnMapHandlers = new Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>>();
        public IReadOnlyDictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>> RequestSpawnMapHandlers => _requestSpawnMapHandlers;
#endif

        public ClusterServer(CentralNetworkManager centralNetworkManager) : base(new LiteNetLibTransport("CLUSTER", 16, 16))
        {
            _centralNetworkManager = centralNetworkManager;
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
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
            RegisterRequestHandler<EmptyMessage, ResponseUserCountMessage>(MMORequestTypes.RequestUserCount, HandleRequestUserCount);
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public bool StartServer()
        {
            return StartServer(_centralNetworkManager.clusterServerPort, int.MaxValue);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        protected override void OnStopServer()
        {
            base.OnStopServer();
            _mapUsersByCharacterId.Clear();
            _connectionIdsByCharacterId.Clear();
            _connectionIdsByCharacterName.Clear();
            _mapSpawnServerPeers.Clear();
            _mapServerPeers.Clear();
            _mapServerPeersByMapId.Clear();
            _mapServerPeersByInstanceId.Clear();
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
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
                    _mapSpawnServerPeers.Remove(eventData.connectionId);
                    // Remove disconnect map server
                    if (_mapServerPeers.TryGetValue(eventData.connectionId, out tempPeerInfo))
                    {
                        _mapServerPeersByMapId.Remove(tempPeerInfo.extra);
                        _mapServerPeersByInstanceId.Remove(tempPeerInfo.extra);
                        _mapServerPeers.Remove(eventData.connectionId);
                        RemoveMapUsers(eventData.connectionId);
                    }
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnPeerNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError + " errorMessage " + eventData.errorMessage);
                    break;
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private void RemoveMapUsers(long connectionId)
        {
            List<KeyValuePair<string, SocialCharacterData>> mapUsers = _mapUsersByCharacterId.ToList();
            foreach (KeyValuePair<string, SocialCharacterData> entry in mapUsers)
            {
                // Find characters which connected to disconnecting map server
                long tempConnectionId;
                if (!_connectionIdsByCharacterId.TryGetValue(entry.Key, out tempConnectionId) || connectionId != tempConnectionId)
                    continue;

                // Send remove messages to other map servers
                UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, entry.Value, connectionId);

                // Clear disconnecting map users data
                _mapUsersByCharacterId.Remove(entry.Key);
                _connectionIdsByCharacterId.Remove(entry.Key);
                _connectionIdsByCharacterName.Remove(entry.Value.characterName);
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private async UniTaskVoid HandleRequestAppServerRegister(
            RequestHandlerData requestHandler,
            RequestAppServerRegisterMessage request,
            RequestProceedResultDelegate<ResponseAppServerRegisterMessage> result)
        {
            await UniTask.Yield();
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            if (request.ValidateHash())
            {
                CentralServerPeerInfo peerInfo = request.peerInfo;
                peerInfo.connectionId = connectionId;
                switch (request.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        _mapSpawnServerPeers[connectionId] = peerInfo;
                        Logging.Log(LogTag, "Register Map Spawn Server: [" + connectionId + "]");
                        break;
                    case CentralServerPeerType.MapServer:
                        // Extra is map ID
                        if (!_mapServerPeersByMapId.ContainsKey(peerInfo.extra))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            _mapServerPeersByMapId[peerInfo.extra] = peerInfo;
                            _mapServerPeers[connectionId] = peerInfo;
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
                        if (!_mapServerPeersByInstanceId.ContainsKey(peerInfo.extra))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Collects server data
                            _mapServerPeersByInstanceId[peerInfo.extra] = peerInfo;
                            _mapServerPeers[connectionId] = peerInfo;
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
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        /// <summary>
        /// This function will be used to send connection information to connected map servers and cluster servers
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="broadcastPeerInfo"></param>
        private void BroadcastAppServers(long connectionId, CentralServerPeerInfo broadcastPeerInfo)
        {
            // Send map peer info to other map server
            foreach (CentralServerPeerInfo mapPeerInfo in _mapServerPeers.Values)
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

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private async UniTaskVoid HandleRequestAppServerAddress(
            RequestHandlerData requestHandler,
            RequestAppServerAddressMessage request,
            RequestProceedResultDelegate<ResponseAppServerAddressMessage> result)
        {
            await UniTask.Yield();
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            switch (request.peerType)
            {
                // TODO: Balancing servers when there are multiple servers with same type
                case CentralServerPeerType.MapSpawnServer:
                    if (_mapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = _mapSpawnServerPeers.Values.First();
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
                    if (!_mapServerPeersByMapId.TryGetValue(mapName, out peerInfo))
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, "Request Map Address: [" + connectionId + "] [" + mapName + "] [" + message + "]");
                    }
                    break;
                case CentralServerPeerType.InstanceMapServer:
                    string instanceId = request.extra;
                    if (!_mapServerPeersByInstanceId.TryGetValue(instanceId, out peerInfo))
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
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
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
                    SendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                    {
                        writer.PutValue(message);
                        writer.Put(messageHandler.Reader.GetString()); // User ID
                        writer.Put(messageHandler.Reader.GetString()); // Access Token
                    });
                    break;
                case ChatChannel.Whisper:
                    long senderConnectionId = 0;
                    long receiverConnectionId = 0;
                    // Send message to map server which have the character
                    if (!string.IsNullOrEmpty(message.senderName) && _connectionIdsByCharacterName.TryGetValue(message.senderName, out senderConnectionId))
                        SendPacket(senderConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    if (!string.IsNullOrEmpty(message.receiverName) && _connectionIdsByCharacterName.TryGetValue(message.receiverName, out receiverConnectionId) && (receiverConnectionId != senderConnectionId))
                        SendPacket(receiverConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    break;
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private void HandleUpdateMapUser(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            SocialCharacterData userData;
            switch (message.type)
            {
                case UpdateUserCharacterMessage.UpdateType.Add:
                    if (!_mapUsersByCharacterId.ContainsKey(message.character.id))
                    {
                        _mapUsersByCharacterId[message.character.id] = message.character;
                        _connectionIdsByCharacterId[message.character.id] = connectionId;
                        _connectionIdsByCharacterName[message.character.characterName] = connectionId;
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Add, message.character, connectionId);
                    }
                    break;
                case UpdateUserCharacterMessage.UpdateType.Remove:
                    if (_mapUsersByCharacterId.TryGetValue(message.character.id, out userData))
                    {
                        _mapUsersByCharacterId.Remove(userData.id);
                        _connectionIdsByCharacterId.Remove(userData.id);
                        _connectionIdsByCharacterName.Remove(userData.characterName);
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, userData, connectionId);
                    }
                    break;
                case UpdateUserCharacterMessage.UpdateType.Online:
                    if (_mapUsersByCharacterId.ContainsKey(message.character.id))
                    {
                        _mapUsersByCharacterId[message.character.id] = message.character;
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Online, message.character, connectionId);
                    }
                    break;
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void HandleUpdatePartyMember(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (_mapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in _mapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdatePartyMember, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void HandleUpdateParty(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (_mapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in _mapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateParty, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void HandleUpdateGuildMember(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (_mapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in _mapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuildMember, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void HandleUpdateGuild(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (_mapServerPeers.ContainsKey(connectionId))
            {
                foreach (long mapServerConnectionId in _mapServerPeers.Keys)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuild, (writer) => writer.PutValue(message));
                }
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void UpdateMapUser(UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData, long exceptConnectionId)
        {
            foreach (long mapServerConnectionId in _mapServerPeers.Keys)
            {
                if (mapServerConnectionId == exceptConnectionId)
                    continue;

                UpdateMapUser(mapServerConnectionId, updateType, userData);
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void UpdateMapUser(long connectionId, UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData)
        {
            UpdateUserCharacterMessage message = new UpdateUserCharacterMessage();
            message.type = updateType;
            message.character = userData;
            SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, (writer) => writer.PutValue(message));
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void PlayerCharacterRemoved(string userId, string characterId)
        {
            List<long> mapServerPeerConnectionIds = new List<long>(_mapServerPeers.Keys);
            foreach (long connectionId in mapServerPeerConnectionIds)
            {
                SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.PlayerCharacterRemoved, (writer) =>
                {
                    writer.Put(userId);
                    writer.Put(characterId);
                });
            }
            List<SocialCharacterData> mapUsers = new List<SocialCharacterData>(_mapUsersByCharacterId.Values);
            foreach (SocialCharacterData mapUser in mapUsers)
            {
                _mapUsersByCharacterId.Remove(userId);
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public void KickUser(string userId, UITextKeys message)
        {
            List<long> mapServerPeerConnectionIds = new List<long>(_mapServerPeers.Keys);
            foreach (long connectionId in mapServerPeerConnectionIds)
            {
                SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.KickUser, (writer) =>
                {
                    writer.Put(userId);
                    writer.PutPackedUShort((ushort)message);
                });
            }
            List<SocialCharacterData> mapUsers = new List<SocialCharacterData>(_mapUsersByCharacterId.Values);
            foreach (SocialCharacterData mapUser in mapUsers)
            {
                _mapUsersByCharacterId.Remove(userId);
            }
        }
#endif

        public bool MapContainsUser(string userId)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            foreach (SocialCharacterData mapUser in _mapUsersByCharacterId.Values)
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
            return SendRequest(connectionId, MMORequestTypes.RequestSpawnMap, message, millisecondsTimeout: _centralNetworkManager.mapSpawnMillisecondsTimeout);
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
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            string requestId = GenericUtils.GetUniqueId();
            request.requestId = requestId;
            List<long> connectionIds = new List<long>(_mapSpawnServerPeers.Keys);
            // Random map-spawn server to spawn map, will use returning ackId as reference to map-server's transport handler and ackId
            RequestSpawnMap(connectionIds[Random.Range(0, connectionIds.Count)], request);
            // Add ack Id / transport handler to dictionary which will be used in OnRequestSpawnMap() function 
            // To send map spawn response to map-server
            _requestSpawnMapHandlers.Add(requestId, result);
#endif
            return default;
        }

        protected void HandleResponseSpawnMap(
            ResponseHandlerData requestHandler,
            AckResponseCode responseCode,
            ResponseSpawnMapMessage response)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Forward responses to map server transport handler
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result;
            if (_requestSpawnMapHandlers.TryGetValue(response.requestId, out result))
                result.Invoke(responseCode, response);
#endif
        }

        /// <summary>
        /// This is function which read request from any server to get online users count (May being used by GM command)
        /// </summary>
        /// <param name="messageHandler"></param>
        protected UniTaskVoid HandleRequestUserCount(
            RequestHandlerData requestHandler,
            EmptyMessage request,
            RequestProceedResultDelegate<ResponseUserCountMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new ResponseUserCountMessage()
            {
                userCount = _mapUsersByCharacterId.Count,
            });
#endif
            return default;
        }

        public async UniTask<int> CountUsers()
        {
            await UniTask.Yield();
            return _mapUsersByCharacterId.Count;
        }

        public async UniTask<CentralServerPeerInfo?> GetMapServer(string mapName)
        {
            await UniTask.Yield();
            if (!_mapServerPeersByMapId.TryGetValue(mapName, out CentralServerPeerInfo mapServerPeerInfo))
                return null;
            return mapServerPeerInfo;
        }

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, long time)
        {
            MD5 algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
