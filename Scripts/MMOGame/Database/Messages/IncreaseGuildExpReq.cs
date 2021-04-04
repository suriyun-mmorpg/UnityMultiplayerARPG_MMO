using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct IncreaseGuildExpReq : INetSerializable
    {
        public int GuildId { get; set; }
        public int Exp { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            Exp = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(Exp);
        }
    }
}