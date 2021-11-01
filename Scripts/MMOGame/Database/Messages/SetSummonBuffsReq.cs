using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct SetSummonBuffsReq : INetSerializable
    {
        public string CharacterId { get; set; }
        public List<CharacterBuff> SummonBuffs { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            SummonBuffs = reader.GetList<CharacterBuff>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.PutList(SummonBuffs);
        }
    }
}
