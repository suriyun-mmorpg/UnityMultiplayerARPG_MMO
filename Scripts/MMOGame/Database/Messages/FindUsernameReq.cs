using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct FindUsernameReq : INetSerializable
    {
        public string Username { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Username = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Username);
        }
    }
}