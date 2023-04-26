using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseAppServerAddressMessage : INetSerializable
    {
        public UITextKeys message;
        public CentralServerPeerInfo peerInfo;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            peerInfo.Serialize(writer);
        }
    }
}
