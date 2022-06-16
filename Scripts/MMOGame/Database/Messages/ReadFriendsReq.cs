using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ReadFriendsReq : INetSerializable
    {
        public string CharacterId { get; set; }
        public bool ReadById2 { get; set; }
        public byte State { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }

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