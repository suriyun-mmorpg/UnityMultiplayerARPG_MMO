using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateClaimMailItemsStateResp : INetSerializable
    {
        public UITextKeys Error { get; set; }
        public Mail Mail { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
            Mail = reader.GetValue<Mail>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
            writer.PutValue(Mail);
        }
    }
}