using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseValidateAccessTokenMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InvalidAccessToken,
        }
        public Error error;
        public string userId;
        public string accessToken;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}
