using LiteNetLib;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class AppRegisterClient : LiteNetLibClient
    {
        public System.Action<AckResponseCode> onAppServerRegistered;

        public bool IsRegisteredToCentralServer { get; private set; }
        public override string LogTag { get { return nameof(AppRegisterClient) + ":" + appServer.PeerType; } }

        private IAppServer appServer;

        public AppRegisterClient(IAppServer appServer) : base(new TcpTransport())
        {
            this.appServer = appServer;
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterResponseHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, OnAppRegistered);
        }

        public override void OnClientReceive(TransportEventData eventData)
        {
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, "OnPeerConnected.");
                    OnCentralServerConnected();
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, "OnClientDisconnected. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    StopClient();
                    OnCentralServerDisconnected(eventData.disconnectInfo).Forget();
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnClientNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError);
                    break;
            }
        }

        public void OnAppStart()
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Starting server");
            ConnectToCentralServer();
        }

        public void OnAppStop()
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Stopping server");
            DisconnectFromCentralServer();
        }

        public void ConnectToCentralServer()
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Connecting to Central Server: " + appServer.CentralNetworkAddress + ":" + appServer.CentralNetworkPort);
            StartClient(appServer.CentralNetworkAddress, appServer.CentralNetworkPort);
        }

        public void DisconnectFromCentralServer()
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Disconnecting from Central Server");
            StopClient();
        }

        public void OnCentralServerConnected()
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Connected to Central Server");
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
        }

        public async UniTaskVoid OnCentralServerDisconnected(DisconnectInfo disconnectInfo)
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Disconnected from Central Server");
            IsRegisteredToCentralServer = false;
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Reconnect to central in 5 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Reconnect to central in 4 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Reconnect to central in 3 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Reconnect to central in 2 seconds...");
            await UniTask.Delay(1000, true);
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Reconnect to central in 1 seconds...");
            await UniTask.Delay(1000, true);
            ConnectToCentralServer();
        }

        public void OnAppRegistered(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerRegisterMessage response)
        {
            if (responseCode == AckResponseCode.Success)
                IsRegisteredToCentralServer = true;
            if (onAppServerRegistered != null)
                onAppServerRegistered.Invoke(responseCode);
        }
    }
}
