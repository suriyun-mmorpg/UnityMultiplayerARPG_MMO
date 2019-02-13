using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestSpawnMap(long connectionId, string sceneName, AckMessageCallback callback)
        {
            RequestSpawnMapMessage message = new RequestSpawnMapMessage();
            message.sceneName = sceneName;
            return Server.ServerSendAckPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.RequestSpawnMap, message, callback);
        }

        protected void HandleResponseSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseSpawnMapMessage message = messageHandler.ReadMessage<ResponseSpawnMapMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected void OnRequestSpawnMap(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            ResponseSpawnMapMessage castedMessage = messageData as ResponseSpawnMapMessage;
            if (LogInfo)
                Debug.Log("Spawn Map Ack Id: " + messageData.ackId + "  Status: " + responseCode + " Error: " + castedMessage.error);
        }
    }
}
