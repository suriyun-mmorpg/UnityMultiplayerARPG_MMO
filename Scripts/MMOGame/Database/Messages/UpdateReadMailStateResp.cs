using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateReadMailStateResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
            Mail = reader.Get(() => new Mail());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
            writer.Put(Mail);
        }
    }
}