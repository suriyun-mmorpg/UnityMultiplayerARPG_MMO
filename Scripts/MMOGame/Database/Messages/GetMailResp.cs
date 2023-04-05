using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetMailResp : INetSerializable
    {
        public Mail Mail { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Mail = reader.Get(() => new Mail());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Mail);
        }
    }
}