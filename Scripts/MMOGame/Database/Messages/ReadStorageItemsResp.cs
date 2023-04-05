using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadStorageItemsResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            StorageCharacterItems = reader.GetList<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(StorageCharacterItems);
        }
    }
}