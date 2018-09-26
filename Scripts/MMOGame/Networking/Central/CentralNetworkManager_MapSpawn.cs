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
        public uint RequestSpawnMap(long connectionId, string sceneName, AckMessageCallback callback)
        {
            var message = new RequestSpawnMapMessage();
            message.sceneName = sceneName;
            return Server.ServerSendAckPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.RequestSpawnMap, message, callback);
        }

        protected void HandleResponseSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseSpawnMapMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void OnRequestSpawnMap(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            var castedMessage = messageData as ResponseSpawnMapMessage;
            Debug.Log("Spawn Map Ack Id: " + messageData.ackId + "  Status: " + responseCode + " Error: " + castedMessage.error);
        }
    }
}
