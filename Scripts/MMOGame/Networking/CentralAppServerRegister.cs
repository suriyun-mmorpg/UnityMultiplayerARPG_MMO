using LiteNetLib;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class CentralAppServerRegister : LiteNetLibClient
    {
        private IAppServer appServer;

        public bool IsRegisteredToCentralServer { get; private set; }

        // Events
        public System.Action<AckResponseCode> onAppServerRegistered;

        public CentralAppServerRegister(ITransport transport, IAppServer appServer) : base(transport)
        {
            this.appServer = appServer;
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterResponseHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.RequestAppServerRegister, OnAppServerRegistered);
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
                    Logging.Log(LogTag, "OnPeerDisconnected. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    StopClient();
                    OnCentralServerDisconnected(eventData.disconnectInfo).Forget();
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError);
                    break;
            }
        }

        public void OnStartServer()
        {
            Logging.Log(LogTag, "[" + appServer.PeerType + "] Starting server");
            ConnectToCentralServer();
        }

        public void OnStopServer()
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
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            peerInfo.peerType = appServer.PeerType;
            peerInfo.networkAddress = appServer.AppAddress;
            peerInfo.networkPort = appServer.AppPort;
            peerInfo.extra = appServer.AppExtra;
            // Send Request
            SendRequest(MMORequestTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
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

        public void OnAppServerRegistered(
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
