using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadStorageItemsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            ReadForUpdate = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.Put(ReadForUpdate);
        }
    }
}