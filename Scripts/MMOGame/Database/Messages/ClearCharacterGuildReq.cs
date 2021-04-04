using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ClearCharacterGuildReq : INetSerializable
    {
        public string CharacterId { get; set; }
        public int GuildId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            GuildId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.Put(GuildId);
        }
    }
}