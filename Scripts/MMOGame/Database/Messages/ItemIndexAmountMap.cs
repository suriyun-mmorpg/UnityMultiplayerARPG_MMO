using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ItemIndexAmountMap : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(Index);
            writer.PutPackedInt(Amount);
        }
        public void Deserialize(NetDataReader reader)
        {
            Index = reader.GetPackedInt();
            Amount = reader.GetPackedInt();
        }
    }
}