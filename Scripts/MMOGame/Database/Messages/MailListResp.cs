using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct MailListResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<MailListEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}