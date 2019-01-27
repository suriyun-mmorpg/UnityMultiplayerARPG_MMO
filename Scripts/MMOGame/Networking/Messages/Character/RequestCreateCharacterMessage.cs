using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestCreateCharacterMessage : BaseAckMessage
    {
        public string characterName;
        public int entityId;
        public int dataId;
        public byte[] extra;

        public override void DeserializeData(NetDataReader reader)
        {
            characterName = reader.GetString();
            entityId = reader.GetInt();
            dataId = reader.GetInt();
            extra = reader.GetBytesWithLength();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(characterName);
            writer.Put(entityId);
            writer.Put(dataId);
            writer.PutBytesWithLength(extra);
        }
    }
}
