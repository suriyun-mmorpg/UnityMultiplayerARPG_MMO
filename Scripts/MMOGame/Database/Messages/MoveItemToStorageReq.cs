using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct MoveItemToStorageReq : INetSerializable
    {
        public string CharacterId { get; set; }
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }
        public int InventoryItemIndex { get; set; }
        public short InventoryItemAmount { get; set; }
        public int StorageItemIndex { get; set; }
        public short WeightLimit { get; set; }
        public short SlotLimit { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            InventoryItemIndex = reader.GetInt();
            InventoryItemAmount = reader.GetShort();
            StorageItemIndex = reader.GetInt();
            WeightLimit = reader.GetShort();
            SlotLimit = reader.GetShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.Put(InventoryItemIndex);
            writer.Put(InventoryItemAmount);
            writer.Put(StorageItemIndex);
            writer.Put(WeightLimit);
            writer.Put(SlotLimit);
        }
    }
}