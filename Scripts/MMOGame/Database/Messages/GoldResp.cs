using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct GoldResp : INetSerializable
    {
        public int Gold { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Gold = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Gold);
        }
    }
}