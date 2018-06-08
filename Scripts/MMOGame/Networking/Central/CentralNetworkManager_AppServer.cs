using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager
    {
        public uint RequestAppServerRegister(CentralServerPeerInfo peerInfo, AckMessageCallback callback)
        {
            var message = new RequestAppServerRegisterMessage();
            message.peerInfo = peerInfo;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MessageTypes.RequestAppServerRegister, message, callback);
        }

        public uint RequestAppServerAddress(CentralServerPeerType peerType, string extra, AckMessageCallback callback)
        {
            var message = new RequestAppServerAddressMessage();
            message.peerType = peerType;
            message.extra = extra;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MessageTypes.RequestAppServerAddress, message, callback);
        }

        protected void HandleRequestAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestAppServerRegisterMessage>();
            var error = ResponseAppServerRegisterMessage.Error.None;
            if (message.ValidateHash())
            {
                var peerInfo = message.peerInfo;
                switch (message.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        mapSpawnServerPeers[peer.ConnectId] = peerInfo;
                        SetupMapSpawn(peerInfo);
                        Debug.Log("[Central] Register Map Spawn Server: [" + peer.ConnectId + "]");
                        break;
                    case CentralServerPeerType.MapServer:
                        var mapName = peerInfo.extra;
                        if (!mapServerPeersByMapName.ContainsKey(mapName))
                        {
                            mapServerPeersByMapName[mapName] = peerInfo;
                            mapServerPeers[peer.ConnectId] = peerInfo;
                            Debug.Log("[Central] Register Map Server: [" + peer.ConnectId + "] [" + mapName + "]");
                        }
                        else
                        {
                            error = ResponseAppServerRegisterMessage.Error.MapAlreadyExisted;
                            Debug.Log("[Central] Register Map Server Failed: [" + peer.ConnectId + "] [" + mapName + "] [" + error + "]");
                        }
                        break;
                }
            }
            else
            {
                error = ResponseAppServerRegisterMessage.Error.InvalidHash;
                Debug.Log("[Central] Register Server Failed: [" + peer.ConnectId + "] [" + error + "]");
            }

            var responseMessage = new ResponseAppServerRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MessageTypes.ResponseAppServerRegister, responseMessage);
        }

        protected void HandleRequestAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestAppServerAddressMessage>();
            var error = ResponseAppServerAddressMessage.Error.None;
            var peerInfo = new CentralServerPeerInfo();
            switch (message.peerType)
            {
                // TODO: Balancing spawner servers
                case CentralServerPeerType.MapSpawnServer:
                    if (mapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = mapSpawnServerPeers.Values.First();
                        Debug.Log("[Central] Request Map Spawn Address: [" + peer.ConnectId + "]");
                    }
                    else
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Map Spawn Address: [" + peer.ConnectId + "] [" + error + "]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    var mapName = message.extra;
                    if (!mapServerPeersByMapName.TryGetValue(mapName, out peerInfo))
                    {
                        error = ResponseAppServerAddressMessage.Error.ServerNotFound;
                        Debug.Log("[Central] Request Map Address: [" + peer.ConnectId + "] [" + mapName + "] [" + error + "]");
                    }
                    break;
            }
            var responseMessage = new ResponseAppServerAddressMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseAppServerAddressMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.peerInfo = peerInfo;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MessageTypes.ResponseAppServerAddress, responseMessage);
        }

        protected void HandleResponseAppServerRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerRegisterMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseAppServerAddress(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, int time)
        {
            // TODO: Add salt
            var algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
