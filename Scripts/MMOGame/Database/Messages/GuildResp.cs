using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GuildResp : INetSerializable
    {
        public GuildData GuildData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildData = reader.Get(() => new GuildData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildData);
        }
    }
}