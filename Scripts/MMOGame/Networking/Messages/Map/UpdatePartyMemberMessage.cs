using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class UpdatePartyMemberMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Remove,
            Online,
        }
        public UpdateType type;
        public int id;
        public string characterId;
        public string characterName;
        public int dataId;
        public int level;
        public int currentHp;
        public int maxHp;
        public int currentMp;
        public int maxMp;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            characterId = reader.GetString();
            if (type == UpdateType.Add || type == UpdateType.Online)
            {
                characterName = reader.GetString();
                dataId = reader.GetInt();
                level = reader.GetInt();
            }
            if (type == UpdateType.Online)
            {
                currentHp = reader.GetInt();
                maxHp = reader.GetInt();
                currentMp = reader.GetInt();
                maxMp = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            writer.Put(characterId);
            if (type == UpdateType.Add || type == UpdateType.Online)
            {
                writer.Put(characterName);
                writer.Put(dataId);
                writer.Put(level);
            }
            if (type == UpdateType.Online)
            {
                writer.Put(currentHp);
                writer.Put(maxHp);
                writer.Put(currentMp);
                writer.Put(maxMp);
            }
        }
    }
}
