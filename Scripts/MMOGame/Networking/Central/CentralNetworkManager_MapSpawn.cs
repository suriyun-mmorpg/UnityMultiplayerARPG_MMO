using System.Collections.Generic;
using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestSpawnMap(long connectionId, string sceneName, string instanceId, AckMessageCallback callback)
        {
            RequestSpawnMapMessage message = new RequestSpawnMapMessage();
            message.mapId = sceneName;
            message.instanceId = instanceId;
            return RequestSpawnMap(connectionId, message, callback);
        }

        public uint RequestSpawnMap(long connectionId, RequestSpawnMapMessage message, AckMessageCallback callback)
        {
            return Server.ServerSendAckPacket(connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.RequestSpawnMap, message, callback);
        }

        /// <summary>
        /// This is function which read request from map server to spawn another map servers
        /// Then it will response back when requested map server is ready
        /// </summary>
        /// <param name="messageHandler"></param>
        protected void HandleRequestSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            RequestSpawnMapMessage message = messageHandler.ReadMessage<RequestSpawnMapMessage>();
            List<long> connectionIds = new List<long>(mapSpawnServerPeers.Keys);
            // Random map-spawn server to spawn map, will use returning ackId as reference to map-server's transport handler and ackId
            uint ackId = RequestSpawnMap(connectionIds[Random.Range(0, connectionIds.Count)], message, OnRequestSpawnMap);
            // Add ack Id / transport handler to dictionary which will be used in OnRequestSpawnMap() function 
            // To send map spawn response to map-server
            requestSpawnMapHandlers.Add(ackId, new KeyValuePair<TransportHandler, uint>(messageHandler.transportHandler, message.ackId));
        }

        protected void OnRequestSpawnMap(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            ResponseSpawnMapMessage castedMessage = messageData as ResponseSpawnMapMessage;
            if (LogInfo)
                Debug.Log("Spawn Map Ack Id: " + messageData.ackId + "  Status: " + responseCode + " Error: " + castedMessage.error);
            // Forward responses to map server transport handler
            KeyValuePair<TransportHandler, uint> requestSpawnMapHandler;
            if (requestSpawnMapHandlers.TryGetValue(castedMessage.ackId, out requestSpawnMapHandler))
                requestSpawnMapHandler.Key.TriggerAck(requestSpawnMapHandler.Value, castedMessage.responseCode, castedMessage);
        }

        /// <summary>
        /// This is function which read response from map spawn server
        /// </summary>
        /// <param name="messageHandler"></param>
        protected void HandleResponseSpawnMap(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseSpawnMapMessage message = messageHandler.ReadMessage<ResponseSpawnMapMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }
    }
}
