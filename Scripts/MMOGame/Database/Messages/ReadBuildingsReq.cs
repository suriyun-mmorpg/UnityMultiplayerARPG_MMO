using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadBuildingsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            MapName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MapName);
        }
    }
}