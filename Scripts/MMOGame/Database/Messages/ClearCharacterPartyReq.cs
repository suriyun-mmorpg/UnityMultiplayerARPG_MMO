using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ClearCharacterPartyReq : INetSerializable
    {
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