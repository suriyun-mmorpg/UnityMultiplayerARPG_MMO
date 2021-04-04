using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ReadStorageItemsReq : INetSerializable
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
        }
    }
}