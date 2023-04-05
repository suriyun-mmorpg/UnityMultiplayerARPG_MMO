using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CreateCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            CharacterData = reader.Get(() => new PlayerCharacterData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(CharacterData);
        }
    }
}