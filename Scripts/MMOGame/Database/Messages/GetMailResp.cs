using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GetMailResp : INetSerializable
    {
        public Mail Mail { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Mail = reader.GetValue<Mail>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutValue(Mail);
        }
    }
}