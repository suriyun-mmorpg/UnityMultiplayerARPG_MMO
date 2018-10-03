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
            ChangeLeader,
            SetGuildMessage,
            SetGuildMemberRole,
            Terminate,
        }
        public UpdateType type;
        public int id;
        public string guildMessage;
        public string characterId;
        public byte guildRole;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            switch (type)
            {
                case UpdateType.ChangeLeader:
                    characterId = reader.GetString();
                    break;
                case UpdateType.SetGuildMessage:
                    guildMessage = reader.GetString();
                    break;
                case UpdateType.SetGuildMemberRole:
                    characterId = reader.GetString();
                    guildRole = reader.GetByte();
                    break;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            switch (type)
            {
                case UpdateType.ChangeLeader:
                    writer.Put(characterId);
                    break;
                case UpdateType.SetGuildMessage:
                    writer.Put(guildMessage);
                    break;
                case UpdateType.SetGuildMemberRole:
                    writer.Put(characterId);
                    writer.Put(guildRole);
                    break;
            }
        }
    }
}
