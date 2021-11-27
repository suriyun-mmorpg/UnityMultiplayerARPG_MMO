using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ClusterServer : LiteNetLibServer, IAppServer
    {
        public string CentralNetworkAddress { get; set; }
        public int CentralNetworkPort { get; set; }
        public string AppAddress { get; set; }
        public int AppPort { get { return ServerPort; } }
        public string AppExtra { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.ClusterServer; } }

        public override string LogTag { get { return nameof(ClusterServer); } }

        private readonly AppRegisterClient appRegisterClient;
        private readonly HashSet<long> mapServerConnectionIds = new HashSet<long>();
        private readonly Dictionary<string, SocialCharacterData> mapUsersById = new Dictionary<string, SocialCharacterData>();
        private readonly Dictionary<string, long> connectionIdsByCharacterId = new Dictionary<string, long>();
        private readonly Dictionary<string, long> connectionIdsByCharacterName = new Dictionary<string, long>();

        public ClusterServer() : base(new TcpTransport())
        {
            appRegisterClient = new AppRegisterClient(this);
            RegisterMessageHandler(MMOMessageTypes.Chat, HandleChatAtServer);
            RegisterMessageHandler(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUserAtServer);
            RegisterMessageHandler(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMemberAtServer);
            RegisterMessageHandler(MMOMessageTypes.UpdateParty, HandleUpdatePartyAtServer);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMemberAtServer);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuild, HandleUpdateGuildAtServer);
        }

        protected override void OnStartServer()
        {
            base.OnStartServer();
            appRegisterClient.OnAppStart();
        }

        protected override void OnStopServer()
        {
            base.OnStopServer();
            mapServerConnectionIds.Clear();
            mapUsersById.Clear();
            connectionIdsByCharacterId.Clear();
            connectionIdsByCharacterName.Clear();
            appRegisterClient.OnAppStop();
        }

        public override void OnServerReceive(TransportEventData eventData)
        {
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, "OnPeerConnected peer.ConnectionId: " + eventData.connectionId);
                    ConnectionIds.Add(eventData.connectionId);
                    if (!mapServerConnectionIds.Contains(eventData.connectionId))
                    {
                        mapServerConnectionIds.Add(eventData.connectionId);
                        // Send add map users
                        foreach (SocialCharacterData userData in mapUsersById.Values)
                        {
                            UpdateMapUser(eventData.connectionId, UpdateUserCharacterMessage.UpdateType.Add, userData);
                        }
                    }
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, "OnPeerDisconnected peer.ConnectionId: " + eventData.connectionId + " disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    ConnectionIds.Remove(eventData.connectionId);
                    if (mapServerConnectionIds.Remove(eventData.connectionId))
                    {
                        SocialCharacterData userData;
                        foreach (KeyValuePair<string, long> entry in connectionIdsByCharacterId)
                        {
                            // Find characters which connected to disconnecting map server
                            if (eventData.connectionId != entry.Value || !mapUsersById.TryGetValue(entry.Key, out userData))
                                continue;

                            // Send remove messages to other map servers
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, userData, eventData.connectionId);
                        }
                    }
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError + " errorMessage " + eventData.errorMessage);
                    Manager.OnPeerNetworkError(eventData.endPoint, eventData.socketError);
                    break;
            }
        }

        private void HandleChatAtServer(MessageHandlerData messageHandler)
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
                    if (!string.IsNullOrEmpty(message.sender) && connectionIdsByCharacterName.TryGetValue(message.sender, out senderConnectionId))
                        SendPacket(senderConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    if (!string.IsNullOrEmpty(message.receiver) && connectionIdsByCharacterName.TryGetValue(message.receiver, out receiverConnectionId) && (receiverConnectionId != senderConnectionId))
                        SendPacket(receiverConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    break;
            }
        }

        private void HandleUpdateMapUserAtServer(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                SocialCharacterData userData;
                switch (message.type)
                {
                    case UpdateUserCharacterMessage.UpdateType.Add:
                        if (!mapUsersById.ContainsKey(message.character.id))
                        {
                            mapUsersById[message.character.id] = message.character;
                            connectionIdsByCharacterId[message.character.id] = connectionId;
                            connectionIdsByCharacterName[message.character.characterName] = connectionId;
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Add, message.character, connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Remove:
                        if (mapUsersById.TryGetValue(message.character.id, out userData))
                        {
                            mapUsersById.Remove(userData.id);
                            connectionIdsByCharacterId.Remove(userData.id);
                            connectionIdsByCharacterName.Remove(userData.characterName);
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, userData, connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Online:
                        if (mapUsersById.ContainsKey(message.character.id))
                        {
                            mapUsersById[message.character.id] = message.character;
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Online, message.character, connectionId);
                        }
                        break;
                }
            }
        }

        private void HandleUpdatePartyMemberAtServer(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdatePartyMember, (writer) => writer.PutValue(message));
                }
            }
        }

        private void HandleUpdatePartyAtServer(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateParty, (writer) => writer.PutValue(message));
                }
            }
        }

        private void HandleUpdateGuildMemberAtServer(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuildMember, (writer) => writer.PutValue(message));
                }
            }
        }

        private void HandleUpdateGuildAtServer(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuild, (writer) => writer.PutValue(message));
                }
            }
        }

        private void UpdateMapUser(UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData, long exceptConnectionId)
        {
            foreach (long mapServerConnectionId in mapServerConnectionIds)
            {
                if (mapServerConnectionId == exceptConnectionId)
                    continue;

                UpdateMapUser(mapServerConnectionId, updateType, userData);
            }
        }

        private void UpdateMapUser(long connectionId, UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData)
        {
            UpdateUserCharacterMessage message = new UpdateUserCharacterMessage();
            message.type = updateType;
            message.character = userData;
            SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, (writer) => writer.PutValue(message));
        }
    }
}
