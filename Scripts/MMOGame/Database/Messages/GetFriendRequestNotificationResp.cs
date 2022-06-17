using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GetFriendRequestNotificationResp : INetSerializable
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