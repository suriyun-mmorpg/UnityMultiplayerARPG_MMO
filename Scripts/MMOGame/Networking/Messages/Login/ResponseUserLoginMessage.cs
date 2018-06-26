using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseUserLoginMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            InvalidUsernameOrPassword,
            AlreadyLogin,
        }
        public Error error;
        public string userId;
        public string accessToken;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}
