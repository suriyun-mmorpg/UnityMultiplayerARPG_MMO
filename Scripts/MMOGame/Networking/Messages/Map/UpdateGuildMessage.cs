using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class UpdateGuildMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            SetGuildMessage,
            Terminate,
        }
        public UpdateType type;
        public int id;
        public string message;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            if (type == UpdateType.SetGuildMessage)
                message = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            if (type == UpdateType.SetGuildMessage)
                writer.Put(message);
        }
    }
}
