using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class UpdateGuildMemberMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Remove,
        }
        public UpdateType type;
        public int id;
        public string characterId;
        public string characterName;
        public int dataId;
        public int level;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            characterId = reader.GetString();
            if (type == UpdateType.Add)
            {
                characterName = reader.GetString();
                dataId = reader.GetInt();
                level = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            writer.Put(characterId);
            if (type == UpdateType.Add)
            {
                writer.Put(characterName);
                writer.Put(dataId);
                writer.Put(level);
            }
        }
    }
}
