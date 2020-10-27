using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseAppServerAddressMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            ServerNotFound,
        }
        public Error error;
        public CentralServerPeerInfo peerInfo;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            peerInfo.Serialize(writer);
        }
    }
}
