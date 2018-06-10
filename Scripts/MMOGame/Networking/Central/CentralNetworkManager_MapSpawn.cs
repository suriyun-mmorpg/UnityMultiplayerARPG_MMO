using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager
    {
        public async void SpawnPublicMaps(NetPeer peer)
        {
            // Await 5 seconds before spawn maps
            await Task.Delay(5050);
            foreach (var scene in MMOServerInstance.Singleton.GetScenes())
            {
                if (!spawningMapAcks.ContainsKey(scene) && !mapServerPeersBySceneName.ContainsKey(scene))
                    spawningMapAcks[scene] = RequestSpawnMap(peer, scene, OnRequestSpawnMap);
            }
        }

        public uint RequestSpawnMap(NetPeer peer, string sceneName, AckMessageCallback callback)
        {
            var message = new RequestSpawnMapMessage();
            message.sceneName = sceneName;
            return Server.SendAckPacket(SendOptions.ReliableUnordered, peer, MessageTypes.RequestSpawnMap, message, callback);
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
