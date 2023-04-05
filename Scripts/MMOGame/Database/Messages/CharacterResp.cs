using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CharacterResp : INetSerializable
    {
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