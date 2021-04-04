using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateGuildMessageReq : INetSerializable
    {
        public int GuildId { get; set; }
        public string GuildMessage { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            GuildMessage = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(GuildMessage);
        }
    }
}