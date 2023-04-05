using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateBuildingReq : INetSerializable
    {
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