using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetSummonBuffsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
        }
    }
}
