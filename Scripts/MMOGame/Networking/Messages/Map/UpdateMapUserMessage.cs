using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class UpdateMapUserMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Remove,
        }
        public UpdateType type;
        public SimpleUserCharacterData userData;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            userData = new SimpleUserCharacterData(reader.GetString(), reader.GetString());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(userData.userId);
            writer.Put(userData.characterName);
        }
    }
}
