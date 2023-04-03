using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct PartyResp : INetSerializable
    {
        public PartyData PartyData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            PartyData = reader.Get(() => new PartyData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PartyData);
        }
    }
}