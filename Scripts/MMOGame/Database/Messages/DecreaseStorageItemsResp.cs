using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct DecreaseStorageItemsResp : INetSerializable
    {
        public UITextKeys Error { get; set; }
        public List<CharacterItem> StorageCharacterItems { get; set; }
        public List<ItemIndexAmountMap> DecreasedItems { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
            StorageCharacterItems = reader.GetList<CharacterItem>();
            DecreasedItems = reader.GetList<ItemIndexAmountMap>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
            writer.PutValue(StorageCharacterItems);
            writer.PutValue(DecreasedItems);
        }
    }
}