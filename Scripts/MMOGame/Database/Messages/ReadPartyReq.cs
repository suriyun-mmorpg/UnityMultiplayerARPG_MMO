using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadPartyReq : INetSerializable
    {
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