using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class UpdateUserCharacterMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Remove,
            Online,
        }
        public UpdateType type;
        public UserCharacterData data = new UserCharacterData();

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            data.id = reader.GetString();
            data.userId = reader.GetString();
            if (type == UpdateType.Add || type == UpdateType.Online)
            {
                data.characterName = reader.GetString();
                data.dataId = reader.GetInt();
                data.level = reader.GetInt();
                data.partyId = reader.GetInt();
                data.guildId = reader.GetInt();
            }
            if (type == UpdateType.Online)
            {
                data.currentHp = reader.GetInt();
                data.maxHp = reader.GetInt();
                data.currentMp = reader.GetInt();
                data.maxMp = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(data.id);
            writer.Put(data.userId);
            if (type == UpdateType.Add || type == UpdateType.Online)
            {
                writer.Put(data.characterName);
                writer.Put(data.dataId);
                writer.Put(data.level);
                writer.Put(data.partyId);
                writer.Put(data.guildId);
            }
            if (type == UpdateType.Online)
            {
                writer.Put(data.currentHp);
                writer.Put(data.maxHp);
                writer.Put(data.currentMp);
                writer.Put(data.maxMp);
            }
        }

        public string CharacterId { get { return data.id; } set { data.id = value; } }
        public string UserId { get { return data.userId; } set { data.userId = value; } }
        public string CharacterName { get { return data.characterName; } set { data.characterName = value; } }
    }
}
