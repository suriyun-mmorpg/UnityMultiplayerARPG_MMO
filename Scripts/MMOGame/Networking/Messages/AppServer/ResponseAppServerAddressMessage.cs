using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
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
            peerInfo.Serialize(writer);
        }
    }
}
