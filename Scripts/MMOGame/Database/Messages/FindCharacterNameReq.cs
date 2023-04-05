using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindCharacterNameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            FinderId = reader.GetString();
            CharacterName = reader.GetString();
            Skip = reader.GetInt();
            Limit = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FinderId);
            writer.Put(CharacterName);
            writer.Put(Skip);
            writer.Put(Limit);
        }
    }
}
