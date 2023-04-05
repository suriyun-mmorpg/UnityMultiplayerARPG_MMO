using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CreateBuildingReq : INetSerializable
    {
        public string MapName { get; set; }
        public BuildingSaveData BuildingData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            MapName = reader.GetString();
            BuildingData = reader.Get(() => new BuildingSaveData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MapName);
            writer.Put(BuildingData);
        }
    }
}