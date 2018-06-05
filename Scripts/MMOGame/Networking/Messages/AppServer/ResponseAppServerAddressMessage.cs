using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseAppServerAddressMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            ServerNotFound,
        }
        public Error error;
        public CentralServerPeerInfo peerInfo;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (peerInfo == null)
                peerInfo = new CentralServerPeerInfo();
            peerInfo.Serialize(writer);
        }
    }
}
