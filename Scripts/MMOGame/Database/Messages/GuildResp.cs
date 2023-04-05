using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GuildResp : INetSerializable
    {
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