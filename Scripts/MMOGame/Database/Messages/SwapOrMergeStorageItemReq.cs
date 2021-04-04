using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct SwapOrMergeStorageItemReq : INetSerializable
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public short WeightLimit { get; set; }
        public short SlotLimit { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            FromIndex = reader.GetInt();
            ToIndex = reader.GetInt();
            WeightLimit = reader.GetShort();
            SlotLimit = reader.GetShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.Put(FromIndex);
            writer.Put(ToIndex);
            writer.Put(WeightLimit);
            writer.Put(SlotLimit);
        }
    }
}