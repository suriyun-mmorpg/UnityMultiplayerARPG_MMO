using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseAppServerRegisterMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InvalidHash,
            MapAlreadyExisted,
            EventAlreadyExisted,
        }
        public Error error;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
        }
    }
}
