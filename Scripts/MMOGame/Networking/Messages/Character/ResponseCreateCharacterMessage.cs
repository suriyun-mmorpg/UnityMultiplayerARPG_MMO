using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseCreateCharacterMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotLoggedin,
            InvalidData,
            TooShortCharacterName,
            TooLongCharacterName,
            CharacterNameAlreadyExisted,
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
