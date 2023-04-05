using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct SetSummonBuffsReq : INetSerializable
    {
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
