using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ChangeGuildGoldReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            ChangeAmount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(ChangeAmount);
        }
    }
}