using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct SetUserUnbanTimeByCharacterNameReq : INetSerializable
    {
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
