using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct CreateGuildReq : INetSerializable
    {
        public string GuildName { get; set; }
        public string LeaderCharacterId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildName = reader.GetString();
            LeaderCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildName);
            writer.Put(LeaderCharacterId);
        }
    }
}