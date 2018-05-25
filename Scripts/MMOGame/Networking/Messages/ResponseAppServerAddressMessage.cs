using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerAddressMessage : ILiteNetLibMessage
    {
        public string error;
        public CentralServerPeerInfo peerInfo;

        public void Deserialize(NetDataReader reader)
        {
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            if (peerInfo == null)
                peerInfo = new CentralServerPeerInfo();
            peerInfo.Serialize(writer);
        }
    }
}
