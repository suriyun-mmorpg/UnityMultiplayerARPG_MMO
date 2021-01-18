using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseUserLoginMessage : INetSerializable
    {
        public UITextKeys error;
        public string userId;
        public string accessToken;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}
