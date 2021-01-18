using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseSpawnMapMessage : INetSerializable
    {
        public UITextKeys error;
        public string instanceId;
        public string requestId;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            instanceId = reader.GetString();
            requestId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            writer.Put(instanceId);
            writer.Put(requestId);
        }
    }
}
