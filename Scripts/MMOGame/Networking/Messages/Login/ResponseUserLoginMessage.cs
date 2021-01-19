using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseUserLoginMessage : INetSerializable
    {
        public UITextKeys message;
        public string userId;
        public string accessToken;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}
