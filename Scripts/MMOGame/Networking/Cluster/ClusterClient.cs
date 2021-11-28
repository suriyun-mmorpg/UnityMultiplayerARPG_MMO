using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class ClusterClient : LiteNetLibClient
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public System.Action<AckResponseCode> onResponseAppServerRegister;
        public System.Action<AckResponseCode, CentralServerPeerInfo> onResponseAppServerAddress;
        public bool IsAppRegistered { get; private set; }
        public override string LogTag { get { return nameof(ClusterClient) + ":" + appServer.PeerType; } }
#endif
        private readonly IAppServer appServer;

        public ClusterClient(IAppServer appServer) : base(new TcpTransport())
        {
            this.appServer = appServer;
#if UNITY_STANDALONE && !CLIENT_BUILD
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterResponseHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, HandleResponseAppServerRegister);
            RegisterResponseHandler<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress, HandleResponseAppServerAddress);
            RegisterMessageHandler(MMOMessageTypes.AppServerAddress, HandleAppServerAddress);
#endif
        }

        public ClusterClient(MapSpawnNetworkManager mapSpawnNetworkManager) : this(mapSpawnNetworkManager as IAppServer)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, mapSpawnNetworkManager.HandleRequestSpawnMap);
#endif
        }

        public override void OnClientReceive(TransportEventData eventData)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
#if UNITY_STANDALONE && !CLIENT_BUILD
            Logging.Log(LogTag, "Starting server");
            ConnectToClusterServer();
#endif
        }

        public void OnAppStop()
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Logging.Log(LogTag, "Stopping server");
            DisconnectFromClusterServer();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void ConnectToClusterServer()
        {
            Logging.Log(LogTag, "Connecting to Cluster Server: " + appServer.ClusterServerAddress + ":" + appServer.ClusterServerPort);
            StartClient(appServer.ClusterServerAddress, appServer.ClusterServerPort);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void DisconnectFromClusterServer()
        {
            Logging.Log(LogTag, "Disconnecting from Cluster Server");
            StopClient();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
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

#if UNITY_STANDALONE && !CLIENT_BUILD
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

#if UNITY_STANDALONE && !CLIENT_BUILD
        public bool RequestAppServerRegister(CentralServerPeerInfo peerInfo)
        {
            Logging.Log(LogTag, "App Register is requesting");
            return SendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
            });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
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

#if UNITY_STANDALONE && !CLIENT_BUILD
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

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleResponseAppServerAddress(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerAddressMessage response)
        {
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(responseCode, response.peerInfo);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void HandleAppServerAddress(MessageHandlerData messageHandler)
        {
            ResponseAppServerAddressMessage response = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(AckResponseCode.Success, response.peerInfo);
        }
#endif
    }
}
