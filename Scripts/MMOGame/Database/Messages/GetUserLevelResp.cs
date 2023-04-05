using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetUserLevelResp : INetSerializable
    {
        public byte UserLevel { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserLevel = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserLevel);
        }
    }
}