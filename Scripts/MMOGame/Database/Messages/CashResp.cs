using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CashResp : INetSerializable
    {
        public int Cash { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Cash = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Cash);
        }
    }
}