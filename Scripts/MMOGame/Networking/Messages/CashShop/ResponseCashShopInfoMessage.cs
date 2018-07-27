using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseCashShopInfoMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            InvalidAccessToken,
        }
        public Error error;
        public int cash;
        public readonly List<NetworkCashShopItem> cashShopItems = new List<NetworkCashShopItem>();

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            cash = reader.GetInt();
            cashShopItems.Clear();
            var size = reader.GetInt();
            for (var i = 0; i < size; ++i)
            {
                var cashShopItem = new NetworkCashShopItem();
                cashShopItem.dataId = reader.GetInt();
                cashShopItem.title = reader.GetString();
                cashShopItem.description = reader.GetString();
                cashShopItem.iconUrl = reader.GetString();
                cashShopItem.sellPrice = reader.GetInt();
                cashShopItems.Add(cashShopItem);
            }
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(cash);
            writer.Put(cashShopItems.Count);
            foreach (var cashShopItem in cashShopItems)
            {
                writer.Put(cashShopItem.dataId);
                writer.Put(cashShopItem.title);
                writer.Put(cashShopItem.description);
                writer.Put(cashShopItem.iconUrl);
                writer.Put(cashShopItem.sellPrice);
            }
        }
    }
}
