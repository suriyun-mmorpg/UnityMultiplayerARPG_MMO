using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateGuildRankReq : INetSerializable
    {
        public int GuildId { get; set; }
        public int Rank { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            Rank = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(Rank);
        }
    }
}