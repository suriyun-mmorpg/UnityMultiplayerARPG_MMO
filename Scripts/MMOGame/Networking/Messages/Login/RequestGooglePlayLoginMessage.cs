using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestGooglePlayLoginMessage : BaseAckMessage
    {
        public string email;
        public string idToken;

        public override void DeserializeData(NetDataReader reader)
        {
            email = reader.GetString();
            idToken = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(email);
            writer.Put(idToken);
        }
    }
}
