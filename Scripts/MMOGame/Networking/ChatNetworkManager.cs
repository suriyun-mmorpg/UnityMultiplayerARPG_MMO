using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG.MMO
{
    public partial class ChatNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        [Header("Central Network Connection")]
        public BaseTransportFactory centralTransportFactory;
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        public BaseDatabase Database
        {
            get { return MMOServerInstance.Singleton.Database; }
        }
        
        public BaseTransportFactory CentralTransportFactory
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // Force to use websocket transport if it's running as webgl
                if (centralTransportFactory == null || !centralTransportFactory.CanUseWithWebGL)
                    centralTransportFactory = gameObject.AddComponent<WebSocketTransportFactory>();
#else
                if (centralTransportFactory == null)
                    centralTransportFactory = gameObject.AddComponent<LiteNetLibTransportFactory>();
#endif
                return centralTransportFactory;
            }
        }

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null && CentralTransportFactory != null)
                    cacheCentralAppServerRegister = new CentralAppServerRegister(CentralTransportFactory.Build(), this);
                return cacheCentralAppServerRegister;
            }
        }

        public string CentralNetworkAddress { get { return centralNetworkAddress; } }
        public int CentralNetworkPort { get { return centralNetworkPort; } }
        public string CentralConnectKey { get { return centralConnectKey; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppConnectKey { get { return connectKey; } }
        public string AppExtra { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.Chat; } }
        private MapNetworkManager mapNetworkManager;
        private readonly HashSet<long> mapServerConnectionIds = new HashSet<long>();
        private readonly Dictionary<string, UserCharacterData> mapUsersById = new Dictionary<string, UserCharacterData>();
        private readonly Dictionary<string, long> connectionIdsByCharacterId = new Dictionary<string, long>();
        private readonly Dictionary<string, long> connectionIdsByCharacterName = new Dictionary<string, long>();

        protected override void RegisterClientMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            base.RegisterClientMessages();
            RegisterClientMessage(MMOMessageTypes.Chat, HandleChatAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUserAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdateParty, HandleUpdatePartyAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMemberAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdateGuild, HandleUpdateGuildAtClient);
        }

        protected override void RegisterServerMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            base.RegisterServerMessages();
            RegisterServerMessage(MMOMessageTypes.Chat, HandleChatAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUserAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMemberAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateParty, HandleUpdatePartyAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMemberAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateGuild, HandleUpdateGuildAtServer);
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            mapNetworkManager = null;
            mapServerConnectionIds.Clear();
            mapUsersById.Clear();
            connectionIdsByCharacterId.Clear();
            connectionIdsByCharacterName.Clear();
        }

        public void StartClient(MapNetworkManager mapNetworkManager, string networkAddress, int networkPort, string connectKey)
        {
            // Start client as map server
            this.mapNetworkManager = mapNetworkManager;
            base.StartClient(networkAddress, networkPort, connectKey);
        }

        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            CentralAppServerRegister.OnStartServer();
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            if (!IsServer)
                Clean();
            CentralAppServerRegister.OnStopServer();
            base.OnStopServer();
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        protected override void Update()
        {
            base.Update();
            if (IsServer)
                CentralAppServerRegister.Update();
        }

        public override void OnPeerConnected(long connectionId)
        {
            base.OnPeerConnected(connectionId);
            if (!mapServerConnectionIds.Contains(connectionId))
            {
                mapServerConnectionIds.Add(connectionId);
                // Send add map users
                foreach (UserCharacterData userData in mapUsersById.Values)
                {
                    UpdateMapUser(connectionId, UpdateUserCharacterMessage.UpdateType.Add, userData);
                }
            }
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(connectionId, disconnectInfo);
            if (mapServerConnectionIds.Remove(connectionId))
            {
                UserCharacterData userData;
                foreach (KeyValuePair<string, long> entry in connectionIdsByCharacterId)
                {
                    // Find characters which connected to disconnecting map server
                    if (connectionId != entry.Value || !mapUsersById.TryGetValue(entry.Key, out userData))
                        continue;

                    // Send remove messages to other map servers
                    UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, userData, connectionId);
                }
            }
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            // Send map users to chat server from map server
            if (mapNetworkManager != null)
                mapNetworkManager.OnChatServerConnected();
        }

        private void HandleChatAtClient(LiteNetLibMessageHandler messageHandler)
        {
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnChatMessageReceive(message);
        }

        private void HandleUpdateMapUserAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdateMapUser(message);
        }

        private void HandleUpdatePartyMemberAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdatePartyMember(message);
        }

        private void HandleUpdatePartyAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdateParty(message);
        }

        private void HandleUpdateGuildMemberAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdateGuildMember(message);
        }

        private void HandleUpdateGuildAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdateGuild(message);
        }

        private void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            if (LogInfo)
                Debug.Log("Handle chat: " + message.channel + " sender: " + message.sender + " receiver: " + message.receiver + " message: " + message.message);
            switch (message.channel)
            {
                case ChatChannel.Global:
                    ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, message);
                    break;
                case ChatChannel.Party:
                case ChatChannel.Guild:
                    // Send message to all map servers, let's map servers filter messages
                    ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, message);
                    break;
                case ChatChannel.Whisper:
                    long senderConnectionId = 0;
                    long receiverConnectionId = 0;
                    // Send message to map server which have the character
                    if (!string.IsNullOrEmpty(message.sender) &&
                        connectionIdsByCharacterName.TryGetValue(message.sender, out senderConnectionId))
                        ServerSendPacket(senderConnectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, message);
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        connectionIdsByCharacterName.TryGetValue(message.receiver, out receiverConnectionId) &&
                        (receiverConnectionId != senderConnectionId))
                        ServerSendPacket(receiverConnectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, message);
                    break;
            }
        }

        private void HandleUpdateMapUserAtServer(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                UserCharacterData userData;
                switch (message.type)
                {
                    case UpdateUserCharacterMessage.UpdateType.Add:
                        if (!mapUsersById.ContainsKey(message.CharacterId))
                        {
                            mapUsersById[message.CharacterId] = message.data;
                            connectionIdsByCharacterId[message.CharacterId] = connectionId;
                            connectionIdsByCharacterName[message.CharacterName] = connectionId;
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Add, message.data, connectionId);
                            if (LogInfo)
                                Debug.Log("[Chat] Add map user: " + message.UserId + " by " + connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Remove:
                        if (mapUsersById.TryGetValue(message.CharacterId, out userData))
                        {
                            mapUsersById.Remove(userData.id);
                            connectionIdsByCharacterId.Remove(userData.id);
                            connectionIdsByCharacterName.Remove(userData.characterName);
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, userData, connectionId);
                            if (LogInfo)
                                Debug.Log("[Chat] Remove map user: " + message.UserId + " by " + connectionId);
                        }
                        break;
                    case UpdateUserCharacterMessage.UpdateType.Online:
                        if (mapUsersById.ContainsKey(message.CharacterId))
                        {
                            mapUsersById[message.CharacterId] = message.data;
                            UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Online, message.data, connectionId);
                            if (LogInfo)
                                Debug.Log("[Chat] Update map user: " + message.UserId + " by " + connectionId);
                        }
                        break;
                }
            }
        }

        private void HandleUpdatePartyMemberAtServer(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        ServerSendPacket(mapServerConnectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdatePartyMember, message);
                }
            }
        }

        private void HandleUpdatePartyAtServer(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        ServerSendPacket(mapServerConnectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateParty, message);
                }
            }
        }

        private void HandleUpdateGuildMemberAtServer(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        ServerSendPacket(mapServerConnectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuildMember, message);
                }
            }
        }

        private void HandleUpdateGuildAtServer(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (mapServerConnectionIds.Contains(connectionId))
            {
                foreach (long mapServerConnectionId in mapServerConnectionIds)
                {
                    if (mapServerConnectionId != connectionId)
                        ServerSendPacket(mapServerConnectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuild, message);
                }
            }
        }

        private void UpdateMapUser(UpdateUserCharacterMessage.UpdateType updateType, UserCharacterData userData, long exceptConnectionId)
        {
            foreach (long mapServerConnectionId in mapServerConnectionIds)
            {
                if (mapServerConnectionId == exceptConnectionId)
                    continue;

                UpdateMapUser(mapServerConnectionId, updateType, userData);
            }
        }

        private void UpdateMapUser(long connectionId, UpdateUserCharacterMessage.UpdateType updateType, UserCharacterData userData)
        {
            UpdateUserCharacterMessage updateMapUserMessage = new UpdateUserCharacterMessage();
            updateMapUserMessage.type = updateType;
            updateMapUserMessage.data = userData;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, updateMapUserMessage);
        }
    }
}
