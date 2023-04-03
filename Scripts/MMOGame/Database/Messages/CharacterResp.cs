using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct CharacterResp : INetSerializable
    {
        public PlayerCharacterData CharacterData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterData = reader.Get(() => new PlayerCharacterData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterData);
        }
    }
}