using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct DeleteBuildingReq : INetSerializable
    {
        public string MapName { get; set; }
        public string BuildingId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            MapName = reader.GetString();
            BuildingId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MapName);
            writer.Put(BuildingId);
        }
    }
}