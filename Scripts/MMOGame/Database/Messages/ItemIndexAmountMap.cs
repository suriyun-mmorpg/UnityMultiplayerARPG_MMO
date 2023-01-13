using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ItemIndexAmountMap : INetSerializable
    {
        public int Index { get; set; }
        public int Amount { get; set; }

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