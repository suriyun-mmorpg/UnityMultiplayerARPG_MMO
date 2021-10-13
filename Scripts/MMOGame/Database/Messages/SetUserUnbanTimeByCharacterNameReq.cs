using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct SetUserUnbanTimeByCharacterNameReq : INetSerializable
    {
        public string CharacterName { get; set; }
        public long UnbanTime { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterName = reader.GetString();
            UnbanTime = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterName);
            writer.PutPackedLong(UnbanTime);
        }
    }
}
