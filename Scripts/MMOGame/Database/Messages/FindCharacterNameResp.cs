using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindCharacterNameResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            FoundAmount = reader.GetLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FoundAmount);
        }
    }
}