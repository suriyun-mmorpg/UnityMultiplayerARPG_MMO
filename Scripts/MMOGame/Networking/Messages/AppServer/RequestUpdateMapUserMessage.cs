using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestUpdateMapUserMessage : BaseAckMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Remove,
        }
        public UpdateType type;
        public string userId;

        public override void DeserializeData(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            userId = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(userId);
        }
    }
}
