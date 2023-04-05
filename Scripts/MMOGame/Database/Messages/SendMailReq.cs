using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct SendMailReq : INetSerializable
    {
        public string ReceiverId { get; set; }
        public Mail Mail { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ReceiverId = reader.GetString();
            Mail = reader.Get(() => new Mail());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ReceiverId);
            writer.Put(Mail);
        }
    }
}