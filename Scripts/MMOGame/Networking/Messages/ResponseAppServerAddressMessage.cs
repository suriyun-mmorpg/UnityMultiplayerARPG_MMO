using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerAddressMessage : ILiteNetLibMessage
    {
        public uint ackId;
        public string error;
        public CentralServerPeerInfo peerInfo;

        public void Deserialize(NetDataReader reader)
        {
            ackId = reader.GetUInt();
            error = reader.GetString();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ackId);
            writer.Put(error);
            if (peerInfo == null)
                peerInfo = new CentralServerPeerInfo();
            peerInfo.Serialize(writer);
        }
    }
}
