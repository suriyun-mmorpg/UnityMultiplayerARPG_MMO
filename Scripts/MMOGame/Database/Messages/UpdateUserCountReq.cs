using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateUserCountReq : INetSerializable
    {
        public int UserCount { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserCount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserCount);
        }
    }
}
