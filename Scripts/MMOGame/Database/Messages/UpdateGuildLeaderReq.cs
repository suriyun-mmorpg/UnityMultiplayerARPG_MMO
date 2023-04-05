using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateGuildLeaderReq : INetSerializable
    {
        public int GuildId { get; set; }
        public string LeaderCharacterId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            LeaderCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(LeaderCharacterId);
        }
    }
}