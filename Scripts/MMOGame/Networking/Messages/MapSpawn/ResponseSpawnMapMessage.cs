using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseSpawnMapMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotReady,
            Unauthorized,
            EmptySceneName,
            CannotExecute,
            Unknow,
        }
        public Error error;
        public string instanceId;
        public string requestId;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            instanceId = reader.GetString();
            requestId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(instanceId);
            writer.Put(requestId);
        }
    }
}
