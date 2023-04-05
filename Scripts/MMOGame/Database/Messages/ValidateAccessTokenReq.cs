using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ValidateAccessTokenReq : INetSerializable
    {
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