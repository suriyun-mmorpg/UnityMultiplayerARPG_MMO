using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestCreateCharacterMessage : BaseAckMessage
    {
        public string characterName;
        public int dataId;

        public override void DeserializeData(NetDataReader reader)
        {
            characterName = reader.GetString();
            dataId = reader.GetInt();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(characterName);
            writer.Put(dataId);
        }
    }
}
