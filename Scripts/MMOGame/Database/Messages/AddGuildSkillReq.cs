using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct AddGuildSkillReq : INetSerializable
    {
        public int GuildId { get; set; }
        public int SkillId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            SkillId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(SkillId);
        }
    }
}