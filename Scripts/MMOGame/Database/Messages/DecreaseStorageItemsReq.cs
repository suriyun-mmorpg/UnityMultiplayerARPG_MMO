using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct DecreaseStorageItemsReq : INetSerializable
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }
        public short WeightLimit { get; set; }
        public short SlotLimit { get; set; }
        public int DataId { get; set; }
        public short Amount { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            WeightLimit = reader.GetShort();
            SlotLimit = reader.GetShort();
            WeightLimit = reader.GetShort();
            DataId = reader.GetInt();
            Amount = reader.GetShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.Put(WeightLimit);
            writer.Put(SlotLimit);
            writer.Put(DataId);
            writer.Put(Amount);
        }
    }
}