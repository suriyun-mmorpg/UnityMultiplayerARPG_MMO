using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct CreateCharacterReq : INetSerializable
    {
        public string UserId { get; set; }
        public PlayerCharacterData CharacterData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            CharacterData = reader.GetValue<PlayerCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.PutValue(CharacterData);
        }
    }
}