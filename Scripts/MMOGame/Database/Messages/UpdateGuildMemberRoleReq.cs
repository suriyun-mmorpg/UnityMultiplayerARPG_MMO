using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateGuildMemberRoleReq : INetSerializable
    {
        public int GuildId { get; set; }
        public byte GuildRole { get; set; }
        public string MemberCharacterId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            GuildRole = reader.GetByte();
            MemberCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(GuildRole);
            writer.Put(MemberCharacterId);
        }
    }
}