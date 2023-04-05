using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct IncreaseGuildExpReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            Level = reader.GetInt();
            Exp = reader.GetInt();
            SkillPoint = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(Level);
            writer.Put(Exp);
            writer.Put(SkillPoint);
        }
    }
}