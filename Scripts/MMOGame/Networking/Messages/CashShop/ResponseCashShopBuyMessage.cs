using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseCashShopBuyMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            InvalidAccessToken,
            ItemNotFound,
        }
        public Error error;
        public int receiveGold;
        public readonly List<NetworkItemAmount> receiveItems = new List<NetworkItemAmount>();

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            receiveGold = reader.GetInt();
            receiveItems.Clear();
            var size = reader.GetInt();
            for (var i = 0; i < size; ++i)
            {
                var receiveItem = new NetworkItemAmount();
                receiveItem.dataId = reader.GetInt();
                receiveItem.amount = reader.GetShort();
                receiveItems.Add(receiveItem);
            }
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(receiveGold);
            writer.Put(receiveItems.Count);
            foreach (var receiveItem in receiveItems)
            {
                writer.Put(receiveItem.dataId);
                writer.Put(receiveItem.amount);
            }
        }
    }
}
