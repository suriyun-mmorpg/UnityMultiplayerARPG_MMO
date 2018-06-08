using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ChatMessage : BaseAckMessage
    {
        public enum Type : byte
        {
            Global,
            Whisper,
            Party,
            Guild,
        }
        public Type type;
        public string receiver;
        public string sender;
        public string message;

        public override void DeserializeData(NetDataReader reader)
        {
            type = (Type)reader.GetByte();
            receiver = reader.GetString();
            sender = reader.GetString();
            message = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(receiver);
            writer.Put(sender);
            writer.Put(message);
        }
    }
}
