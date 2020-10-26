using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestSpawnMap(long connectionId, string sceneName, string instanceId, Vector3 instanceWarpPosition, bool instanceWarpOverrideRotation, Vector3 instanceWarpRotation, AckMessageCallback<ResponseSpawnMapMessage> callback)
        {
            return RequestSpawnMap(connectionId, new RequestSpawnMapMessage()
            {
                mapId = sceneName,
                instanceId = instanceId,
                instanceWarpPosition = instanceWarpPosition,
                instanceWarpOverrideRotation = instanceWarpOverrideRotation,
                instanceWarpRotation = instanceWarpRotation,
            }, callback);
        }

        public uint RequestSpawnMap(long connectionId, RequestSpawnMapMessage message, AckMessageCallback<ResponseSpawnMapMessage> callback)
        {
            return ServerSendRequest(connectionId, MMOMessageTypes.RequestSpawnMap, message, callback, duration: mapSpawnDuration);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
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
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected void OnRequestSpawnMap(ResponseSpawnMapMessage messageData)
        {
            if (messageData.responseCode == AckResponseCode.Timeout)
            {
                if (LogError)
                    Logging.Log(LogTag, "Spawn Map Ack Id: " + messageData.ackId + " Timeout.");
                return;
            }
            if (LogInfo)
                Logging.Log(LogTag, "Spawn Map Ack Id: " + messageData.ackId + "  Status: " + messageData.responseCode + " Error: " + messageData.error);
            // Forward responses to map server transport handler
            NetDataWriter forwardWriter = new NetDataWriter();
            messageData.Serialize(forwardWriter);
            KeyValuePair<TransportHandler, uint> requestSpawnMapHandler;
            if (requestSpawnMapHandlers.TryGetValue(messageData.ackId, out requestSpawnMapHandler))
                requestSpawnMapHandler.Key.ReadResponse(new NetDataReader(forwardWriter.Data));
        }
#endif
    }
}
