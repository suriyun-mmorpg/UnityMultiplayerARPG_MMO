using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class ClusterClient : LiteNetLibClient
    {
        public override string LogTag { get { return nameof(ClusterClient) + ":" + appServer.PeerType; } }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public System.Action<AckResponseCode> onResponseAppServerRegister;
        public System.Action<AckResponseCode, CentralServerPeerInfo> onResponseAppServerAddress;
        public bool IsAppRegistered { get; private set; }
        private readonly IAppServer appServer;
#endif

        public ClusterClient(IAppServer appServer) : base(new TcpTransport())
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            this.appServer = appServer;
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterResponseHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, HandleResponseAppServerRegister);
            RegisterResponseHandler<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.RequestAppServerAddress, HandleResponseAppServerAddress);
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

        private void ConnectToClusterServer()
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Logging.Log(LogTag, "Connecting to Cluster Server: " + appServer.ClusterServerAddress + ":" + appServer.ClusterServerPort);
            StartClient(appServer.ClusterServerAddress, appServer.ClusterServerPort);
#endif
        }

        private void DisconnectFromClusterServer()
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Logging.Log(LogTag, "Disconnecting from Cluster Server");
            StopClient();
#endif
        }

        private void OnConnectedToClusterServer()
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Logging.Log(LogTag, "Connected to Cluster Server");
            // Send Request
            SendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = new CentralServerPeerInfo()
                {
                    peerType = appServer.PeerType,
                    networkAddress = appServer.AppAddress,
                    networkPort = appServer.AppPort,
                    extra = appServer.AppExtra,
                },
            });
#endif
        }

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
            return SendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
            });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public void HandleResponseAppServerRegister(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerRegisterMessage response)
        {
            if (responseCode == AckResponseCode.Success)
                IsAppRegistered = true;
            if (onResponseAppServerRegister != null)
                onResponseAppServerRegister.Invoke(responseCode);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public bool RequestAppServerAddress(CentralServerPeerType peerType, string extra)
        {
            return SendRequest(MMORequestTypes.RequestAppServerAddress, new RequestAppServerAddressMessage()
            {
                peerType = peerType,
                extra = extra,
            });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public void HandleResponseAppServerAddress(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerAddressMessage response)
        {
            if (responseCode == AckResponseCode.Success)
                IsAppRegistered = true;
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(responseCode, response.peerInfo);
        }
#endif
    }
}
