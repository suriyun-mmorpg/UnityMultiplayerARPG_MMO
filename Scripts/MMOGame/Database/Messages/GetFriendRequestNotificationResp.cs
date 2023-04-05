using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetFriendRequestNotificationResp : INetSerializable
    {
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