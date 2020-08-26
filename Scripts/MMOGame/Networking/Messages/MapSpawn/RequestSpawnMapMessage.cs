using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class RequestSpawnMapMessage : BaseAckMessage
    {
        public string mapId;
        public string instanceId;
        public Vector3 instanceWarpPosition;
        public bool instanceWarpOverrideRotation;
        public Vector3 instanceWarpRotation;

        public override void DeserializeData(NetDataReader reader)
        {
            mapId = reader.GetString();
            instanceId = reader.GetString();
            instanceWarpPosition = reader.GetVector3();
            instanceWarpOverrideRotation = reader.GetBool();
            instanceWarpRotation = reader.GetVector3();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(mapId);
            writer.Put(instanceId);
            writer.PutVector3(instanceWarpPosition);
            writer.Put(instanceWarpOverrideRotation);
            writer.PutVector3(instanceWarpRotation);
        }
    }
}
