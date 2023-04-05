using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CashResp : INetSerializable
    {
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