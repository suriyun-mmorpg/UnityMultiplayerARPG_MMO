using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GetMailReq : INetSerializable
    {
        public string MailId { get; set; }
        public string UserId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            MailId = reader.GetString();
            UserId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MailId);
            writer.Put(UserId);
        }
    }
}