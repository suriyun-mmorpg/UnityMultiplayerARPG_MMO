using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadFriendsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            ReadById2 = reader.GetBool();
            State = reader.GetByte();
            Skip = reader.GetInt();
            Limit = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.Put(ReadById2);
            writer.Put(State);
            writer.Put(Skip);
            writer.Put(Limit);
        }
    }
}