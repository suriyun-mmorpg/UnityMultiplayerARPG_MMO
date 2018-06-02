using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerRegistrationMessage : ILiteNetLibMessage
    {
        public uint ackId;
        public string error;

        public void Deserialize(NetDataReader reader)
        {
            ackId = reader.GetUInt();
            error = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ackId);
            writer.Put(error);
        }
    }
}
