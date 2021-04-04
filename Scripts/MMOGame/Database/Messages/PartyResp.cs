using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct PartyResp : INetSerializable
    {
        public PartyData PartyData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            PartyData = reader.GetValue<PartyData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutValue(PartyData);
        }
    }
}