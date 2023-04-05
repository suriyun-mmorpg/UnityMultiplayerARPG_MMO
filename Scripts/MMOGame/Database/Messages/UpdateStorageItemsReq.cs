using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateStorageItemsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            StorageItems = reader.GetList<CharacterItem>();
            UpdateCharacterData = reader.GetBool();
            if (UpdateCharacterData)
                CharacterData = reader.Get(() => new PlayerCharacterData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.PutList(StorageItems);
            writer.Put(UpdateCharacterData);
            if (UpdateCharacterData)
                writer.Put(CharacterData);
        }
    }
}
