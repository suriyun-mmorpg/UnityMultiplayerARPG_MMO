using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GuildGoldResp : INetSerializable
    {
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