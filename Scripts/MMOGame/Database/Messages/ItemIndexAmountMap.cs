using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ItemIndexAmountMap : INetSerializable
    {
        public int Index { get; set; }
        public short Amount { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Index);
            writer.Put(Amount);
        }

        public void Deserialize(NetDataReader reader)
        {
            Index = reader.GetInt();
            Amount = reader.GetShort();
        }
    }
}