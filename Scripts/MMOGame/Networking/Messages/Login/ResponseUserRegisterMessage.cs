using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseUserRegisterMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            TooShortUsername,
            TooShortPassword,
            TooLongUsername,
            UsernameAlreadyExisted,
        }
        public Error error;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
        }
    }
}
