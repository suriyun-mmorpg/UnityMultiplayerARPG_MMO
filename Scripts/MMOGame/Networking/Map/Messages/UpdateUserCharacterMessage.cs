using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateUserCharacterMessage : INetSerializable
    {
        public enum UpdateType : byte
        {
            Add,
            Remove,
            Online,
        }
        public UpdateType type;
        public SocialCharacterData character;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            switch (type)
            {
                case UpdateType.Add:
                    character.DeserializeWithoutHpMp(reader);
                    break;
                case UpdateType.Online:
                    character.Deserialize(reader);
                    break;
                case UpdateType.Remove:
                    character.id = reader.GetString();
                    character.userId = reader.GetString();
                    break;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            switch (type)
            {
                case UpdateType.Add:
                    character.SerializeWithoutHpMp(writer);
                    break;
                case UpdateType.Online:
                    character.Serialize(writer);
                    break;
                case UpdateType.Remove:
                    writer.Put(character.id);
                    writer.Put(character.userId);
                    break;
            }
        }
    }
}
