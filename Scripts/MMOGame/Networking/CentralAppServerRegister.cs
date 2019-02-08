using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class CentralAppServerRegister : TransportHandler
    {
        private IAppServer appServer;

        public bool IsRegisteredToCentralServer { get; private set; }

        // Events
        public System.Action<AckResponseCode, BaseAckMessage> onAppServerRegistered;

        public CentralAppServerRegister(ITransport transport, IAppServer appServer) : base(transport, appServer.CentralConnectKey)
        {
            this.appServer = appServer;
            RegisterMessage(MMOMessageTypes.ResponseAppServerRegister, HandleResponseAppServerRegister);
        }

        public override void OnClientReceive(TransportEventData eventData)
        {
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Debug.Log("CentralAppServerRegister::OnPeerConnected.");
                    OnCentralServerConnected();
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Debug.Log("CentralAppServerRegister::OnPeerDisconnected. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    OnCentralServerDisconnected(eventData.disconnectInfo);
                    break;
                case ENetworkEvent.ErrorEvent:
                    Debug.LogError("CentralAppServerRegister::OnNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketErrorCode);
                    break;
            }
        }

        public void OnStartServer()
        {
            Debug.Log("[" + appServer.PeerType + "] Starting server");
            ConnectToCentralServer();
        }

        public void OnStopServer()
        {
            Debug.Log("[" + appServer.PeerType + "] Stopping server");
            DisconnectFromCentralServer();
        }

        public void ConnectToCentralServer()
        {
            Debug.Log("[" + appServer.PeerType + "] Connecting to Central Server: " + appServer.CentralNetworkAddress + ":" + appServer.CentralNetworkPort + " " + appServer.CentralConnectKey);
            StartClient(appServer.CentralNetworkAddress, appServer.CentralNetworkPort);
        }

        public void DisconnectFromCentralServer()
        {
            Debug.Log("[" + appServer.PeerType + "] Disconnecting from Central Server");
            StopClient();
        }

        public void OnCentralServerConnected()
        {
            Debug.Log("[" + appServer.PeerType + "] Connected to Central Server");
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            peerInfo.peerType = appServer.PeerType;
            peerInfo.networkAddress = appServer.AppAddress;
            peerInfo.networkPort = appServer.AppPort;
            peerInfo.connectKey = appServer.AppConnectKey;
            peerInfo.extra = appServer.AppExtra;
            // Send Request
            RequestAppServerRegisterMessage message = new RequestAppServerRegisterMessage();
            message.peerInfo = peerInfo;
            ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestAppServerRegister, message, OnAppServerRegistered);
        }

        public async void OnCentralServerDisconnected(DisconnectInfo disconnectInfo)
        {
            Debug.Log("[" + appServer.PeerType + "] Disconnected from Central Server");
            IsRegisteredToCentralServer = false;
            await Task.Delay(500);
            Debug.Log("[" + appServer.PeerType + "] Reconnect to central in 5 seconds...");
            await Task.Delay(1000);
            Debug.Log("[" + appServer.PeerType + "] Reconnect to central in 4 seconds...");
            await Task.Delay(1000);
            Debug.Log("[" + appServer.PeerType + "] Reconnect to central in 3 seconds...");
            await Task.Delay(1000);
            Debug.Log("[" + appServer.PeerType + "] Reconnect to central in 2 seconds...");
            await Task.Delay(1000);
            Debug.Log("[" + appServer.PeerType + "] Reconnect to central in 1 seconds...");
            await Task.Delay(1000);
            ConnectToCentralServer();
        }

        private void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler peerHandler = messageHandler.transportHandler;
            ResponseAppServerRegisterMessage message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            uint ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        public void OnAppServerRegistered(AckResponseCode responseCode, BaseAckMessage message)
        {
            if (responseCode == AckResponseCode.Success)
                IsRegisteredToCentralServer = true;
            if (onAppServerRegistered != null)
                onAppServerRegistered(responseCode, message);
        }
    }
}
