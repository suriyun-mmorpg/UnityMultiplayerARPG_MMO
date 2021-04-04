using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct SendMailResp : INetSerializable
    {
        public UITextKeys Error { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
        }
    }
}