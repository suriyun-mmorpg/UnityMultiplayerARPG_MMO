using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct FindGuildNameReq : INetSerializable
    {
        public string GuildName { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildName);
        }
    }
}