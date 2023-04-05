using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateGuildRoleReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            GuildRole = reader.GetByte();
            GuildRoleData = reader.Get<GuildRoleData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(GuildRole);
            writer.Put(GuildRoleData);
        }
    }
}