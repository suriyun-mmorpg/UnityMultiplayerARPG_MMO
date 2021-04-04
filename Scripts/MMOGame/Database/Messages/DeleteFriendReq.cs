using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct DeleteFriendReq : INetSerializable
    {
        public string Character1Id { get; set; }
        public string Character2Id { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Character1Id = reader.GetString();
            Character2Id = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Character1Id);
            writer.Put(Character2Id);
        }
    }
}