using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public bool RequestSpawnMap(long connectionId, string sceneName, string instanceId, Vector3 instanceWarpPosition, bool instanceWarpOverrideRotation, Vector3 instanceWarpRotation)
        {
            return RequestSpawnMap(connectionId, new RequestSpawnMapMessage()
            {
                mapId = sceneName,
                instanceId = instanceId,
                instanceWarpPosition = instanceWarpPosition,
                instanceWarpOverrideRotation = instanceWarpOverrideRotation,
                instanceWarpRotation = instanceWarpRotation,
            });
        }

        public bool RequestSpawnMap(long connectionId, RequestSpawnMapMessage message)
        {
            return ServerSendRequest(connectionId, MMORequestTypes.RequestSpawnMap, message, duration: mapSpawnDuration);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        /// <summary>
        /// This is function which read request from map server to spawn another map servers
        /// Then it will response back when requested map server is ready
        /// </summary>
        /// <param name="messageHandler"></param>
        protected UniTaskVoid HandleRequestSpawnMap(
            RequestHandlerData requestHandler,
            RequestSpawnMapMessage request,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result)
        {
            string requestId = GenericUtils.GetUniqueId();
            request.requestId = requestId;
            List<long> connectionIds = new List<long>(mapSpawnServerPeers.Keys);
            // Random map-spawn server to spawn map, will use returning ackId as reference to map-server's transport handler and ackId
            RequestSpawnMap(connectionIds[Random.Range(0, connectionIds.Count)], request);
            // Add ack Id / transport handler to dictionary which will be used in OnRequestSpawnMap() function 
            // To send map spawn response to map-server
            requestSpawnMapHandlers.Add(requestId, result);
            return default;
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected UniTaskVoid HandleResponseSpawnMap(
            ResponseHandlerData requestHandler,
            AckResponseCode responseCode,
            ResponseSpawnMapMessage response)
        {
            // Forward responses to map server transport handler
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result;
            if (requestSpawnMapHandlers.TryGetValue(response.requestId, out result))
                result.Invoke(responseCode, response);
            return default;
        }
#endif
    }
}
