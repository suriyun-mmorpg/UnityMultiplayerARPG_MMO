using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestUserRegisterMessage : INetSerializable
    {
        public string username;
        public string password;
        public string email;

        public void Deserialize(NetDataReader reader)
        {
            username = reader.GetString();
            password = reader.GetString();
            email = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(username);
            writer.Put(password);
            writer.Put(email);
        }
    }
}
