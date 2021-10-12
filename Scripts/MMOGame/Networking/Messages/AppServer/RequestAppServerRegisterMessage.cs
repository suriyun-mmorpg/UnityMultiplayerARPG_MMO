using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestAppServerRegisterMessage : INetSerializable
    {
        public CentralServerPeerInfo peerInfo;
        public long time { get; private set; }
        public string hash { get; private set; }

        public void Deserialize(NetDataReader reader)
        {
            peerInfo.Deserialize(reader);
            time = reader.GetPackedLong();
            hash = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            time = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            hash = CentralNetworkManager.GetAppServerRegisterHash(peerInfo.peerType, time);
            peerInfo.Serialize(writer);
            writer.PutPackedLong(time);
            writer.Put(hash);
        }

        public bool ValidateHash()
        {
            if (string.IsNullOrEmpty(hash))
                return false;
            return hash.Equals(CentralNetworkManager.GetAppServerRegisterHash(peerInfo.peerType, time));
        }
    }
}
