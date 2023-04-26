using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseValidateAccessTokenMessage : INetSerializable
    {
        public UITextKeys message;
        public string userId;
        public string accessToken;
        public long unbanTime;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            userId = reader.GetString();
            accessToken = reader.GetString();
            unbanTime = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.Put(userId);
            writer.Put(accessToken);
            writer.PutPackedLong(unbanTime);
        }
    }
}
