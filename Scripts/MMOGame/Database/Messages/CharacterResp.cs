using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct CharacterResp : INetSerializable
    {
        public PlayerCharacterData CharacterData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterData = reader.GetValue<PlayerCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutValue(CharacterData);
        }
    }
}