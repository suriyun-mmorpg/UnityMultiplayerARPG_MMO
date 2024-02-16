namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        private void PrepareMapHandlers()
        {
            // Server Handlers
            ServerMailHandlers = gameObject.GetOrAddComponent<IServerMailHandlers, MMOServerMailHandlers>();
            ServerUserHandlers = gameObject.GetOrAddComponent<IServerUserHandlers, MMOServerUserHandlers>();
            ServerBuildingHandlers = gameObject.GetOrAddComponent<IServerBuildingHandlers, DefaultServerBuildingHandlers>();
            ServerCharacterHandlers = gameObject.GetOrAddComponent<IServerCharacterHandlers, DefaultServerCharacterHandlers>();
            ServerGameMessageHandlers = gameObject.GetOrAddComponent<IServerGameMessageHandlers, DefaultServerGameMessageHandlers>();
            ServerStorageHandlers = gameObject.GetOrAddComponent<IServerStorageHandlers, MMOServerStorageHandlers>();
            ServerPartyHandlers = gameObject.GetOrAddComponent<IServerPartyHandlers, DefaultServerPartyHandlers>();
            ServerGuildHandlers = gameObject.GetOrAddComponent<IServerGuildHandlers, MMOServerGuildHandlers>();
            ServerChatHandlers = gameObject.GetOrAddComponent<IServerChatHandlers, DefaultServerChatHandlers>();
            ServerLogHandlers = gameObject.GetOrAddComponent<IServerLogHandlers, DefaultServerLogHandlers>();
            // Server Message Handlers
            ServerCashShopMessageHandlers = gameObject.GetOrAddComponent<IServerCashShopMessageHandlers, MMOServerCashShopMessageHandlers>();
            ServerMailMessageHandlers = gameObject.GetOrAddComponent<IServerMailMessageHandlers, MMOServerMailMessageHandlers>();
            ServerStorageMessageHandlers = gameObject.GetOrAddComponent<IServerStorageMessageHandlers, MMOServerStorageMessageHandlers>();
            ServerCharacterMessageHandlers = gameObject.GetOrAddComponent<IServerCharacterMessageHandlers, DefaultServerCharacterMessageHandlers>();
            ServerInventoryMessageHandlers = gameObject.GetOrAddComponent<IServerInventoryMessageHandlers, DefaultServerInventoryMessageHandlers>();
            ServerPartyMessageHandlers = gameObject.GetOrAddComponent<IServerPartyMessageHandlers, MMOServerPartyMessageHandlers>();
            ServerGuildMessageHandlers = gameObject.GetOrAddComponent<IServerGuildMessageHandlers, MMOServerGuildMessageHandlers>();
            ServerGachaMessageHandlers = gameObject.GetOrAddComponent<IServerGachaMessageHandlers, MMOServerGachaMessageHandlers>();
            ServerFriendMessageHandlers = gameObject.GetOrAddComponent<IServerFriendMessageHandlers, MMOServerFriendMessageHandlers>();
            ServerBankMessageHandlers = gameObject.GetOrAddComponent<IServerBankMessageHandlers, MMOServerBankMessageHandlers>();
            ServerOnlineCharacterMessageHandlers = gameObject.GetOrAddComponent<IServerOnlineCharacterMessageHandlers, DefaultServerOnlineCharacterMessageHandlers>();
            // Client handlers
            ClientCashShopHandlers = gameObject.GetOrAddComponent<IClientCashShopHandlers, DefaultClientCashShopHandlers>();
            ClientMailHandlers = gameObject.GetOrAddComponent<IClientMailHandlers, DefaultClientMailHandlers>();
            ClientStorageHandlers = gameObject.GetOrAddComponent<IClientStorageHandlers, DefaultClientStorageHandlers>();
            ClientCharacterHandlers = gameObject.GetOrAddComponent<IClientCharacterHandlers, DefaultClientCharacterHandlers>();
            ClientInventoryHandlers = gameObject.GetOrAddComponent<IClientInventoryHandlers, DefaultClientInventoryHandlers>();
            ClientPartyHandlers = gameObject.GetOrAddComponent<IClientPartyHandlers, DefaultClientPartyHandlers>();
            ClientGuildHandlers = gameObject.GetOrAddComponent<IClientGuildHandlers, DefaultClientGuildHandlers>();
            ClientGachaHandlers = gameObject.GetOrAddComponent<IClientGachaHandlers, DefaultClientGachaHandlers>();
            ClientFriendHandlers = gameObject.GetOrAddComponent<IClientFriendHandlers, DefaultClientFriendHandlers>();
            ClientBankHandlers = gameObject.GetOrAddComponent<IClientBankHandlers, DefaultClientBankHandlers>();
            ClientUserContentHandlers = gameObject.GetOrAddComponent<IClientUserContentHandlers, DefaultClientUserContentHandlers>();
            ClientOnlineCharacterHandlers = gameObject.GetOrAddComponent<IClientOnlineCharacterHandlers, DefaultClientOnlineCharacterHandlers>();
            ClientChatHandlers = gameObject.GetOrAddComponent<IClientChatHandlers, DefaultClientChatHandlers>();
            ClientGameMessageHandlers = gameObject.GetOrAddComponent<IClientGameMessageHandlers, DefaultClientGameMessageHandlers>();
        }
    }
}
