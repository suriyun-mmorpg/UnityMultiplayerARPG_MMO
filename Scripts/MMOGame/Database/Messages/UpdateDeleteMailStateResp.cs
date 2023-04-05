using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateDeleteMailStateResp : INetSerializable
    {
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