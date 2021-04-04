using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ReadBuildingsReq : INetSerializable
    {
        public string MapName { get; set; }

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