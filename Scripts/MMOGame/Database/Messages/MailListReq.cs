using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct MailListReq : INetSerializable
    {
        public string UserId { get; set; }
        public bool OnlyNewMails { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            OnlyNewMails = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(OnlyNewMails);
        }
    }
}