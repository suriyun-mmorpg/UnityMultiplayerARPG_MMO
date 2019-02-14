using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestSpawnMapMessage : BaseAckMessage
    {
        public string mapId;
        public string instanceId;

        public override void DeserializeData(NetDataReader reader)
        {
            mapId = reader.GetString();
            instanceId = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(mapId);
            writer.Put(instanceId);
        }
    }
}
