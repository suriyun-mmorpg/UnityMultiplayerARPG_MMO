using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ChangeGoldReq : INetSerializable
    {
        public string UserId { get; set; }
        public int ChangeAmount { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            ChangeAmount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(ChangeAmount);
        }
    }
}