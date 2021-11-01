using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct GetSummonBuffsResp : INetSerializable
    {
        public List<CharacterBuff> SummonBuffs { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            SummonBuffs = reader.GetList<CharacterBuff>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(SummonBuffs);
        }
    }
}
