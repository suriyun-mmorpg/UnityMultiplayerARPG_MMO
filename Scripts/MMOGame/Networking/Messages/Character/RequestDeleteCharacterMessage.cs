using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestDeleteCharacterMessage : INetSerializable
    {
        public string characterId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
        }
    }
}
