using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GetIdByCharacterNameResp : INetSerializable
    {
        public string Id { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
        }
    }
}