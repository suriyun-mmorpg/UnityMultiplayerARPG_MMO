using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseUserCountMessage : INetSerializable
    {
        public UITextKeys message;
        public int userCount;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            userCount = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutPackedInt(userCount);
        }
    }
}
