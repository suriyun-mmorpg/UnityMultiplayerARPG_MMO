using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdatePartyLeaderReq : INetSerializable
    {
        public int PartyId { get; set; }
        public string LeaderCharacterId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            PartyId = reader.GetInt();
            LeaderCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PartyId);
            writer.Put(LeaderCharacterId);
        }
    }
}