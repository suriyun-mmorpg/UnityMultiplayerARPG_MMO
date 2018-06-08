using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestSpawnMapMessage : BaseAckMessage
    {
        public string sceneName;

        public override void DeserializeData(NetDataReader reader)
        {
            sceneName = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(sceneName);
        }
    }
}
