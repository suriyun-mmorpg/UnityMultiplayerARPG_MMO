using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseAppServerAddressMessage : INetSerializable
    {
        public UITextKeys error;
        public CentralServerPeerInfo peerInfo;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            peerInfo.Serialize(writer);
        }
    }
}
