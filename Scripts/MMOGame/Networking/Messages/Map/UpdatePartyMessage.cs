using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class UpdatePartyMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Setting,
            Terminate,
        }
        public UpdateType type;
        public int id;
        public bool shareExp;
        public bool shareItem;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            if (type == UpdateType.Setting)
            {
                shareExp = reader.GetBool();
                shareItem = reader.GetBool();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            if (type == UpdateType.Setting)
            {
                writer.Put(shareExp);
                writer.Put(shareItem);
            }
        }
    }
}
