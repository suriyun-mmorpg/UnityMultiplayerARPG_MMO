using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ClearCharacterPartyReq : INetSerializable
    {
        public string CharacterId { get; set; }
        public int PartyId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            PartyId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.Put(PartyId);
        }
    }
}