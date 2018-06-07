using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestSelectCharacterMessage : BaseAckMessage
    {
        public string characterId;

        public override void DeserializeData(NetDataReader reader)
        {
            characterId = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(characterId);
        }
    }
}
