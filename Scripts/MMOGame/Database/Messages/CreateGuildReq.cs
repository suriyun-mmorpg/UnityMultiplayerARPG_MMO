using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CreateGuildReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildName = reader.GetString();
            LeaderCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildName);
            writer.Put(LeaderCharacterId);
        }
    }
}