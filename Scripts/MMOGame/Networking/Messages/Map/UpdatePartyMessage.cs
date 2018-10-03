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
            ChangeLeader,
            Setting,
            Terminate,
        }
        public UpdateType type;
        public int id;
        public bool shareExp;
        public bool shareItem;
        public string characterId;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            switch (type)
            {
                case UpdateType.ChangeLeader:
                    characterId = reader.GetString();
                    break;
                case UpdateType.Setting:
                shareExp = reader.GetBool();
                shareItem = reader.GetBool();
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
                case UpdateType.Setting:
                    writer.Put(shareExp);
                    writer.Put(shareItem);
                    break;
            }
        }
    }
}
