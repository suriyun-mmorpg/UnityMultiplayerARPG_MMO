using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct PartyResp : INetSerializable
    {
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