using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class MMOWarpMessage : INetSerializable
    {
        public string networkAddress;
        public int networkPort;

        public void Deserialize(NetDataReader reader)
        {
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(networkAddress);
            writer.Put(networkPort);
        }
    }
}
