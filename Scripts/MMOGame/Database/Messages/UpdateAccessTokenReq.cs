using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateAccessTokenReq : INetSerializable
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            AccessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(AccessToken);
        }
    }
}