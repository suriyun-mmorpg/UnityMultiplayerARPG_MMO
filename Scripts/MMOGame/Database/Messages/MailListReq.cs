using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct MailListReq : INetSerializable
    {
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