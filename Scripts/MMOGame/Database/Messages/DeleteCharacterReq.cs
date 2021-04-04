using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct DeleteCharacterReq : INetSerializable
    {
        public string UserId { get; set; }
        public string CharacterId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            CharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(CharacterId);
        }
    }
}