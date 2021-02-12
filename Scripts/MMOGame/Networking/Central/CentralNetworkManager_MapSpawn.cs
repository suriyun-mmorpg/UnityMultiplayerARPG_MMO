using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
            return ServerSendRequest(connectionId, MMORequestTypes.RequestSpawnMap, message, millisecondsTimeout: mapSpawnMillisecondsTimeout);
        }

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
#if UNITY_STANDALONE && !CLIENT_BUILD
            string requestId = GenericUtils.GetUniqueId();
            request.requestId = requestId;
            List<long> connectionIds = new List<long>(mapSpawnServerPeers.Keys);
            // Random map-spawn server to spawn map, will use returning ackId as reference to map-server's transport handler and ackId
            RequestSpawnMap(connectionIds[Random.Range(0, connectionIds.Count)], request);
            // Add ack Id / transport handler to dictionary which will be used in OnRequestSpawnMap() function 
            // To send map spawn response to map-server
            requestSpawnMapHandlers.Add(requestId, result);
#endif
            return default;
        }

        protected void HandleResponseSpawnMap(
            ResponseHandlerData requestHandler,
            AckResponseCode responseCode,
            ResponseSpawnMapMessage response)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Forward responses to map server transport handler
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result;
            if (requestSpawnMapHandlers.TryGetValue(response.requestId, out result))
                result.Invoke(responseCode, response);
#endif
        }
    }
}
