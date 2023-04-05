using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct DeleteBuildingReq : INetSerializable
    {
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