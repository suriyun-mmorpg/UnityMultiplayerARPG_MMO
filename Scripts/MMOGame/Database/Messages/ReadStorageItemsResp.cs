using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadStorageItemsResp : INetSerializable
    {
        public List<CharacterItem> StorageCharacterItems { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            StorageCharacterItems = reader.GetList<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(StorageCharacterItems);
        }
    }
}