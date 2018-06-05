using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerAddressMessage : BaseAckMessage
    {
        public string error;
        public CentralServerPeerInfo peerInfo;

        public override void DeserializeData(NetDataReader reader)
        {
            error = reader.GetString();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(error);
            if (peerInfo == null)
                peerInfo = new CentralServerPeerInfo();
            peerInfo.Serialize(writer);
        }
    }
}
