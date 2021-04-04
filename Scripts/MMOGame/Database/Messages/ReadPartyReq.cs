using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ReadPartyReq : INetSerializable
    {
        public int PartyId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            PartyId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PartyId);
        }
    }
}