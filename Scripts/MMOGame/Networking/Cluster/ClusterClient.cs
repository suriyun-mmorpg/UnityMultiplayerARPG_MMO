using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class ClusterClient : LiteNetLibClient
    {
        public override string LogTag { get { return nameof(ClusterClient); } }

        private readonly MapNetworkManager mapNetworkManager;

        public ClusterClient(MapNetworkManager mapNetworkManager) : base(new TcpTransport())
        {
            this.mapNetworkManager = mapNetworkManager;
            RegisterMessageHandler(MMOMessageTypes.Chat, HandleChatAtClient);
            RegisterMessageHandler(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUserAtClient);
            RegisterMessageHandler(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            RegisterMessageHandler(MMOMessageTypes.UpdateParty, HandleUpdatePartyAtClient);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMemberAtClient);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuild, HandleUpdateGuildAtClient);
        }

        public override void OnClientReceive(TransportEventData eventData)
        {
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, "OnClientConnected");
                    mapNetworkManager.OnClusterServerConnected();
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(-1, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, "OnPeerDisconnected peer. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    StopClient();
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError + " errorMessage " + eventData.errorMessage);
                    break;
            }
        }

        private void HandleChatAtClient(MessageHandlerData messageHandler)
        {
            mapNetworkManager.OnChatMessageReceive(messageHandler.ReadMessage<ChatMessage>());
        }

        private void HandleUpdateMapUserAtClient(MessageHandlerData messageHandler)
        {
            mapNetworkManager.OnUpdateMapUser(messageHandler.ReadMessage<UpdateUserCharacterMessage>());
        }

        private void HandleUpdatePartyMemberAtClient(MessageHandlerData messageHandler)
        {
            mapNetworkManager.OnUpdatePartyMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
        }

        private void HandleUpdatePartyAtClient(MessageHandlerData messageHandler)
        {
            mapNetworkManager.OnUpdateParty(messageHandler.ReadMessage<UpdatePartyMessage>());
        }

        private void HandleUpdateGuildMemberAtClient(MessageHandlerData messageHandler)
        {
            mapNetworkManager.OnUpdateGuildMember(messageHandler.ReadMessage<UpdateSocialMemberMessage>());
        }

        private void HandleUpdateGuildAtClient(MessageHandlerData messageHandler)
        {
            mapNetworkManager.OnUpdateGuild(messageHandler.ReadMessage<UpdateGuildMessage>());
        }
    }
}
