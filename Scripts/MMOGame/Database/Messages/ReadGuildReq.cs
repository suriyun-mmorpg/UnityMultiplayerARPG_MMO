using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ReadGuildReq : INetSerializable
    {
        public int GuildId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
        }
    }
}