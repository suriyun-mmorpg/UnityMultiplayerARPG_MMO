using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestValidateAccessTokenMessage : INetSerializable
    {
        public string userId;
        public string accessToken;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}
