using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestAppServerAddressMessage : ILiteNetLibMessage
    {
        public uint ackId;
        public CentralServerPeerType peerType;
        public string extra;

        public void Deserialize(NetDataReader reader)
        {
            ackId = reader.GetUInt();
            peerType = (CentralServerPeerType)reader.GetByte();
            extra = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ackId);
            writer.Put((byte)peerType);
            writer.Put(extra);
        }
    }
}
