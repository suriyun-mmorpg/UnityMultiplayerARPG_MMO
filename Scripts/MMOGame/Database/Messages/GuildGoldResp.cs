using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GuildGoldResp : INetSerializable
    {
        public int GuildGold { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildGold = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildGold);
        }
    }
}