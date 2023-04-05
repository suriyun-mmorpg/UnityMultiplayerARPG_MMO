using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindEmailReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Email = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Email);
        }
    }
}