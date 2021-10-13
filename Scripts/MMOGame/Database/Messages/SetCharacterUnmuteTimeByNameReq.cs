using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct SetCharacterUnmuteTimeByNameReq : INetSerializable
    {
        public string CharacterName { get; set; }
        public long UnmuteTime { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterName = reader.GetString();
            UnmuteTime = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterName);
            writer.PutPackedLong(UnmuteTime);
        }
    }
}
