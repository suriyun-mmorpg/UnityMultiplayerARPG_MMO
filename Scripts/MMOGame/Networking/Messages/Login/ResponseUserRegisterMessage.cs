using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseUserRegisterMessage : INetSerializable
    {
        public UITextKeys message;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
        }
    }
}
