using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateGuildRoleReq : INetSerializable
    {
        public int GuildId { get; set; }
        public byte GuildRole { get; set; }
        public string RoleName { get; set; }
        public bool CanInvite { get; set; }
        public bool CanKick { get; set; }
        public byte ShareExpPercentage { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            GuildRole = reader.GetByte();
            RoleName = reader.GetString();
            CanInvite = reader.GetBool();
            CanKick = reader.GetBool();
            ShareExpPercentage = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(GuildRole);
            writer.Put(RoleName);
            writer.Put(CanInvite);
            writer.Put(CanKick);
            writer.Put(ShareExpPercentage);
        }
    }
}