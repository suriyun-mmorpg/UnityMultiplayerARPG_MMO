using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseUserRegisterMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            TooShortUsername,
            TooLongUsername,
            TooShortPassword,
            UsernameAlreadyExisted,
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
