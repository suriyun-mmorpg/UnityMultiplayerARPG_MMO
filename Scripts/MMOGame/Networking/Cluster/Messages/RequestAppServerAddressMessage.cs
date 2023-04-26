using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestAppServerAddressMessage : INetSerializable
    {
        public CentralServerPeerType peerType;
        public string extra;

        public void Deserialize(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            extra = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(extra);
        }
    }
}
