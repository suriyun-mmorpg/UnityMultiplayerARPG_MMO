using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class ResponseSelectCharacterMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotLoggedin,
            AlreadySelectCharacter,
            InvalidCharacterId,
            MapNotReady,
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
