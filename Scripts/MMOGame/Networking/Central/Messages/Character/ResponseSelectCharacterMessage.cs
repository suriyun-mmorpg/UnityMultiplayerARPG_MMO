using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseSelectCharacterMessage : INetSerializable
    {
        public UITextKeys message;
        public string sceneName;
        public string networkAddress;
        public int networkPort;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            sceneName = reader.GetString();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.Put(sceneName);
            writer.Put(networkAddress);
            writer.Put(networkPort);
        }
    }
}
