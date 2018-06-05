using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestUserLoginMessage : BaseAckMessage
    {
        public string username;
        public string password;

        public override void DeserializeData(NetDataReader reader)
        {
            username = reader.GetString();
            password = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(username);
            writer.Put(password);
        }
    }
}
