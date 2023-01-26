using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class ClusterClient : LiteNetLibClient
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public System.Action<AckResponseCode> onResponseAppServerRegister;
        public System.Action<AckResponseCode, CentralServerPeerInfo> onResponseAppServerAddress;
        public System.Action<AckResponseCode, int> onResponseUserCount;
        public bool IsAppRegistered { get; private set; }
        public override string LogTag { get { return nameof(ClusterClient) + ":" + appServer.PeerType; } }
#endif
        private readonly IAppServer appServer;

        public ClusterClient(IAppServer appServer) : base(new LiteNetLibTransport("CLUSTER", 16, 16))
        {
            this.appServer = appServer;
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterResponseHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, HandleResponseAppServerRegister);
            RegisterResponseHandler<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress, HandleResponseAppServerAddress);
            RegisterResponseHandler<EmptyMessage, ResponseUserCountMessage>(MMORequestTypes.RequestUserCount, HandleResponseUserCount);
            RegisterMessageHandler(MMOMessageTypes.AppServerAddress, HandleAppServerAddress);
            RegisterMessageHandler(MMOMessageTypes.KickUser, HandleKickUser);
#endif
        }

        public ClusterClient(MapSpawnNetworkManager mapSpawnNetworkManager) : this(mapSpawnNetworkManager as IAppServer)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, mapSpawnNetworkManager.HandleRequestSpawnMap);
#endif
        }

        public override void OnClientReceive(TransportEventData eventData)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, "OnClientConnected");
                    OnConnectedToClusterServer();
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(-1, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, "OnClientDisconnected peer. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    StopClient();
                    OnDisconnectedFromClusterServer().Forget();
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnClientNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError + " errorMessage " + eventData.errorMessage);
                    break;
            }
#endif
        }

        public void OnAppStart()
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            Logging.Log(LogTag, "Starting server");
            ConnectToClusterServer();
#endif
        }

        public void OnAppStop()
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            Logging.Log(LogTag, "Stopping server");
            DisconnectFromClusterServer();
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void ConnectToClusterServer()
        {
            Logging.Log(LogTag, "Connecting to Cluster Server: " + appServer.ClusterServerAddress + ":" + appServer.ClusterServerPort);
            StartClient(appServer.ClusterServerAddress, appServer.ClusterServerPort);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void DisconnectFromClusterServer()
        {
            Logging.Log(LogTag, "Disconnecting from Cluster Server");
            StopClient();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void OnConnectedToClusterServer()
        {
            Logging.Log(LogTag, "Connected to Cluster Server");
            // Send Request
            RequestAppServerRegister(new CentralServerPeerInfo()
            {
                peerType = appServer.PeerType,
                networkAddress = appServer.AppAddress,
                networkPort = appServer.AppPort,
                extra = appServer.AppExtra,
            });
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTaskVoid OnDisconnectedFromClusterServer()
        {
            Logging.Log(LogTag, "Disconnected from Central Server");
            IsAppRegistered = false;
            Logging.Log(LogTag, "Reconnect to central in 5 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "Reconnect to central in 4 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "Reconnect to central in 3 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "Reconnect to central in 2 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "Reconnect to central in 1 seconds...");
            await UniTask.Delay(1000, true);
            ConnectToClusterServer();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public bool RequestAppServerRegister(CentralServerPeerInfo peerInfo)
        {
            Logging.Log(LogTag, "App Register is requesting");
            return SendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
            });
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void HandleResponseAppServerRegister(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerRegisterMessage response)
        {
            if (responseCode == AckResponseCode.Success)
            {
                Logging.Log(LogTag, "App Registered successfully");
                IsAppRegistered = true;
            }
            if (onResponseAppServerRegister != null)
                onResponseAppServerRegister.Invoke(responseCode);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public bool RequestAppServerAddress(CentralServerPeerType peerType, string extra)
        {
            Logging.Log(LogTag, "App Address is requesting");
            return SendRequest(MMORequestTypes.RequestAppServerAddress, new RequestAppServerAddressMessage()
            {
                peerType = peerType,
                extra = extra,
            });
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void HandleResponseAppServerAddress(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerAddressMessage response)
        {
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(responseCode, response.peerInfo);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void HandleResponseUserCount(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseUserCountMessage response)
        {
            if (onResponseUserCount != null)
                onResponseUserCount.Invoke(responseCode, response.userCount);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void HandleAppServerAddress(MessageHandlerData messageHandler)
        {
            ResponseAppServerAddressMessage response = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(AckResponseCode.Success, response.peerInfo);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void HandleKickUser(MessageHandlerData messageHandler)
        {
            string kickUserId = messageHandler.Reader.GetString();
            UITextKeys message = (UITextKeys)messageHandler.Reader.GetPackedUShort();
            if (appServer is MapNetworkManager mapNetworkManager)
                mapNetworkManager.KickUser(kickUserId, message);
        }
#endif
    }
}
