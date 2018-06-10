using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestValidateAccessToken : BaseAckMessage
    {
        public string userId;
        public string accessToken;

        public override void DeserializeData(NetDataReader reader)
        {
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}
