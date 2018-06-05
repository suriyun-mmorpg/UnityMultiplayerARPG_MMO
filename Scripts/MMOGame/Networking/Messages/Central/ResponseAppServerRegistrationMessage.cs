using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerRegistrationMessage : BaseAckMessage
    {
        public string error;

        public override void DeserializeData(NetDataReader reader)
        {
            error = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(error);
        }
    }
}
