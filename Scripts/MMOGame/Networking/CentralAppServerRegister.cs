using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public class CentralAppServerRegister : LiteNetLibPeerHandler
    {
        private IAppServer appServer;

        public bool IsRegisteredToCentralServer { get; private set; }
        public NetPeer Peer { get; protected set; }
        public bool IsConnected { get { return Peer != null && Peer.ConnectionState == ConnectionState.Connected; } }

        // Events
        public System.Action<AckResponseCode, BaseAckMessage> onAppServerRegistered;

        public CentralAppServerRegister(IAppServer appServer) : base(1, appServer.CentralConnectKey)
        {
            this.appServer = appServer;
            RegisterMessage(MessageTypes.ResponseAppServerRegister, HandleResponseAppServerRegister);
        }

        public override void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            base.OnNetworkError(endPoint, socketErrorCode);
            Debug.LogError("CentralAppServerRegister::OnNetworkError endPoint: " + endPoint + " socketErrorCode " + socketErrorCode);
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            base.OnNetworkLatencyUpdate(peer, latency);
        }

        public override void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            base.OnNetworkReceive(peer, reader);
        }

        public override void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            base.OnNetworkReceiveUnconnected(remoteEndPoint, reader, messageType);
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            base.OnPeerConnected(peer);
            Debug.Log("CentralAppServerRegister::OnPeerConnected peer.ConnectId: " + peer.ConnectId);
            OnCentralServerConnected(peer);
            Peer = peer;
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(peer, disconnectInfo);
            Debug.Log("CentralAppServerRegister::OnPeerDisconnected peer.ConnectId: " + peer.ConnectId + " disconnectInfo.Reason: " + disconnectInfo.Reason);
            OnCentralServerDisconnected(peer, disconnectInfo);
            Peer = null;
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
            Debug.Log("[" + appServer.PeerType + "] Connecting to Central Server");
            Start();
            Connect(appServer.CentralNetworkAddress, appServer.CentralNetworkPort);
        }

        public void DisconnectFromCentralServer()
        {
            Debug.Log("[" + appServer.PeerType + "] Disconnecting from Central Server");
            Stop();
        }

        public void OnCentralServerConnected(NetPeer netPeer)
        {
            Debug.Log("[" + appServer.PeerType + "] Connected to Central Server");
            var peerInfo = new CentralServerPeerInfo();
            peerInfo.peer = netPeer;
            peerInfo.peerType = appServer.PeerType;
            peerInfo.networkAddress = appServer.AppAddress;
            peerInfo.networkPort = appServer.AppPort;
            peerInfo.connectKey = appServer.AppConnectKey;
            peerInfo.extra = appServer.AppExtra;
            // Send Request
            var message = new RequestAppServerRegisterMessage();
            message.peerInfo = peerInfo;
            SendAckPacket(SendOptions.ReliableUnordered, netPeer, MessageTypes.RequestAppServerRegister, message, OnAppServerRegistered);
        }

        public async void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("[" + appServer.PeerType + "] Disconnected from Central Server");
            IsRegisteredToCentralServer = false;
            Debug.Log("[" + appServer.PeerType + "] Reconnect to central in 5 seconds");
            await Task.Delay(5000);
            ConnectToCentralServer();
        }

        private void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            var ackId = message.ackId;
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
