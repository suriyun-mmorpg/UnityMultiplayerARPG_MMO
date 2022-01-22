using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestUserLoginMessage : INetSerializable
    {
        public string username;
        public string password;

        public void Deserialize(NetDataReader reader)
        {
            username = reader.GetString();
            password = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(username);
            writer.Put(password);
        }
    }
}
