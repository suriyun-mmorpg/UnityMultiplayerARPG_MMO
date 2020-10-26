using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public System.Action<AckResponseCode, BaseAckMessage> onAppServerRegistered;

        public CentralAppServerRegister(ITransport transport, IAppServer appServer) : base(transport)
        {
            this.appServer = appServer;
            RegisterMessage(MMOMessageTypes.GenericResponse, HandleGenericResponse);
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
            SendRequest<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMOMessageTypes.RequestAppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
            }, OnAppServerRegistered);
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

        private void HandleGenericResponse(LiteNetLibMessageHandler messageHandler)
        {
            messageHandler.ReadResponse();
        }

        public void OnAppServerRegistered(ResponseAppServerRegisterMessage message)
        {
            if (message.responseCode == AckResponseCode.Success)
                IsRegisteredToCentralServer = true;
            if (onAppServerRegistered != null)
                onAppServerRegistered.Invoke(message.responseCode, message);
        }
    }
}