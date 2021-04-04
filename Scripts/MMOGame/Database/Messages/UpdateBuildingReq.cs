using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdateBuildingReq : INetSerializable
    {
        public string MapName { get; set; }
        public BuildingSaveData BuildingData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            MapName = reader.GetString();
            BuildingData = reader.GetValue<BuildingSaveData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MapName);
            writer.PutValue(BuildingData);
        }
    }
}