using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerRegistrationMessage : ILiteNetLibMessage
    {
        public string error;

        public void Deserialize(NetDataReader reader)
        {
            error = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(error);
        }
    }
}
