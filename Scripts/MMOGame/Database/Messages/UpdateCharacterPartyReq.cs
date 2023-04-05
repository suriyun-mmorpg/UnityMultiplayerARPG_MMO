using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateCharacterPartyReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            PartyId = reader.GetInt();
            SocialCharacterData = reader.Get<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PartyId);
            writer.Put(SocialCharacterData);
        }
    }
}