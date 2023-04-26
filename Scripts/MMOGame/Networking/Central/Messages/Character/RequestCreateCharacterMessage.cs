using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestCreateCharacterMessage : INetSerializable
    {
        public string characterName;
        public int entityId;
        public int dataId;
        public int factionId;

        public void Deserialize(NetDataReader reader)
        {
            characterName = reader.GetString();
            entityId = reader.GetInt();
            dataId = reader.GetInt();
            factionId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterName);
            writer.Put(entityId);
            writer.Put(dataId);
            writer.Put(factionId);
        }
    }
}
