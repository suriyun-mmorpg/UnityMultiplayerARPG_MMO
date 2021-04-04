using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct SendMailReq : INetSerializable
    {
        public string ReceiverId { get; set; }
        public Mail Mail { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ReceiverId = reader.GetString();
            Mail = reader.GetValue<Mail>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ReceiverId);
            writer.PutValue(Mail);
        }
    }
}