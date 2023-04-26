using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseSpawnMapMessage : INetSerializable
    {
        public UITextKeys message;
        public string instanceId;
        public string requestId;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            instanceId = reader.GetString();
            requestId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.Put(instanceId);
            writer.Put(requestId);
        }
    }
}
