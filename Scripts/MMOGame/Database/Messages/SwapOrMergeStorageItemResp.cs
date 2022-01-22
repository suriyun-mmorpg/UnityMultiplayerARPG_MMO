using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct SwapOrMergeStorageItemResp : INetSerializable
    {
        public UITextKeys Error { get; set; }
        public List<CharacterItem> StorageCharacterItems { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
            StorageCharacterItems = reader.GetList<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
            writer.PutList(StorageCharacterItems);
        }
    }
}