using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GuildResp : INetSerializable
    {
        public GuildData GuildData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildData = reader.GetValue<GuildData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutValue(GuildData);
        }
    }
}