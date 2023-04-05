using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct DeleteFriendReq : INetSerializable
    {
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