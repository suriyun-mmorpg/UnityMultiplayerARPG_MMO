using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GetMailNotificationResp : INetSerializable
    {
        public int NotificationCount { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            NotificationCount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NotificationCount);
        }
    }
}