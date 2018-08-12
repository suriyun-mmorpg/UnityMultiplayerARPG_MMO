using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestSpawnMap(NetPeer peer, string sceneName, AckMessageCallback callback)
        {
            var message = new RequestSpawnMapMessage();
            message.sceneName = sceneName;
            return Server.SendAckPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.RequestSpawnMap, message, callback);
        }

        protected void HandleResponseSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseSpawnMapMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void OnRequestSpawnMap(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            var castedMessage = messageData as ResponseSpawnMapMessage;
            Debug.Log("Spawn Map Ack Id: " + messageData.ackId + "  Status: " + responseCode + " Error: " + castedMessage.error);
        }
    }
}
