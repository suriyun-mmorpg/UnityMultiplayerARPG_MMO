using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateGuildAutoAcceptRequestsReq : INetSerializable
    {
        public int GuildId { get; set; }
        public bool AutoAcceptRequests { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            AutoAcceptRequests = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(AutoAcceptRequests);
        }
    }
}