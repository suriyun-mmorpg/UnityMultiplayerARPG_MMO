using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestCashShopBuyMessage : BaseAckMessage
    {
        public string userId;
        public string accessToken;
        public int dataId;

        public override void DeserializeData(NetDataReader reader)
        {
            userId = reader.GetString();
            accessToken = reader.GetString();
            dataId = reader.GetInt();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(accessToken);
            writer.Put(dataId);
        }
    }
}
