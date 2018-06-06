using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestCreateCharacterMessage : BaseAckMessage
    {
        public string characterName;
        public string databaseId;

        public override void DeserializeData(NetDataReader reader)
        {
            characterName = reader.GetString();
            databaseId = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(characterName);
            writer.Put(databaseId);
        }
    }
}
