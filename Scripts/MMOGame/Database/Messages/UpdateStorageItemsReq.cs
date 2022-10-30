using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct UpdateStorageItemsReq : INetSerializable
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }
        public List<CharacterItem> StorageItems { get; set; }
        public bool UpdateCharacterData { get; set; }
        public PlayerCharacterData CharacterData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            StorageItems = reader.GetList<CharacterItem>();
            UpdateCharacterData = reader.GetBool();
            if (UpdateCharacterData)
                CharacterData = reader.GetValue<PlayerCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.PutList(StorageItems);
            writer.Put(UpdateCharacterData);
            if (UpdateCharacterData)
                writer.PutValue(CharacterData);
        }
    }
}
