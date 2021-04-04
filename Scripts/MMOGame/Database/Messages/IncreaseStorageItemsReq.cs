using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct IncreaseStorageItemsReq : INetSerializable
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }
        public short WeightLimit { get; set; }
        public short SlotLimit { get; set; }
        public CharacterItem Item { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            WeightLimit = reader.GetShort();
            SlotLimit = reader.GetShort();
            Item = reader.GetValue<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.Put(WeightLimit);
            writer.Put(SlotLimit);
            writer.PutValue(Item);
        }
    }
}