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
            bool hasValue = reader.GetBool();
            if (hasValue)
                CharacterData = reader.Get(() => new PlayerCharacterData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.PutList(StorageItems);
            bool hasValue = CharacterData != null;
            writer.Put(hasValue);
            if (hasValue)
                writer.Put(CharacterData);
        }
    }
}
