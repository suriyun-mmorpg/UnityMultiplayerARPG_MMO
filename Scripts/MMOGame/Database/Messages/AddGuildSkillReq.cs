using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct AddGuildSkillReq : INetSerializable
    {
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