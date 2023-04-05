using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateDeleteMailStateReq : INetSerializable
    {
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