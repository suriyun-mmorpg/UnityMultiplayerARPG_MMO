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
        public List<CashShopItemInfo> cashShopItems;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            cash = reader.GetInt();
            cashShopItems = new List<CashShopItemInfo>();
            var size = reader.GetInt();
            for (var i = 0; i < size; ++i)
            {
                var cashShopItem = new CashShopItemInfo();
                cashShopItem.networkDataId = reader.GetInt();
                cashShopItem.externalIconUrl = reader.GetString();
                cashShopItem.sellPrice = reader.GetInt();
            }
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(cash);
            writer.Put(cashShopItems.Count);
            foreach (var cashShopItem in cashShopItems)
            {
                writer.Put(cashShopItem.networkDataId);
                writer.Put(cashShopItem.externalIconUrl);
                writer.Put(cashShopItem.sellPrice);
            }
        }
    }
}
