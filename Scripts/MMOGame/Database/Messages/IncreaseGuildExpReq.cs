using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct IncreaseGuildExpReq : INetSerializable
    {
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