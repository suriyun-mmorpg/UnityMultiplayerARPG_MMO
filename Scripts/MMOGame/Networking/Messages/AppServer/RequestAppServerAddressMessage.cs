using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestAppServerAddressMessage : BaseAckMessage
    {
        public CentralServerPeerType peerType;
        public string extra;

        public override void DeserializeData(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            extra = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(extra);
        }
    }
}
