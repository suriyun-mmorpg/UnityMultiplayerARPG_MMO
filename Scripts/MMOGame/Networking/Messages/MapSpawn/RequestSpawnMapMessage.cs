using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public struct RequestSpawnMapMessage : INetSerializable
    {
        public string mapId;
        public string instanceId;
        public Vector3 instanceWarpPosition;
        public bool instanceWarpOverrideRotation;
        public Vector3 instanceWarpRotation;
        public string requestId;

        public void Deserialize(NetDataReader reader)
        {
            mapId = reader.GetString();
            instanceId = reader.GetString();
            instanceWarpPosition = reader.GetVector3();
            instanceWarpOverrideRotation = reader.GetBool();
            instanceWarpRotation = reader.GetVector3();
            requestId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(mapId);
            writer.Put(instanceId);
            writer.PutVector3(instanceWarpPosition);
            writer.Put(instanceWarpOverrideRotation);
            writer.PutVector3(instanceWarpRotation);
            writer.Put(requestId);
        }
    }
}
