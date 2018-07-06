using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestFacebookLoginMessage : BaseAckMessage
    {
        public string id;
        public string accessToken;

        public override void DeserializeData(NetDataReader reader)
        {
            id = reader.GetString();
            accessToken = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(accessToken);
        }
    }
}
