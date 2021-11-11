using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct FindEmailReq : INetSerializable
    {
        public string Email { get; set; }

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