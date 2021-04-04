using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ValidateUserLoginReq : INetSerializable
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Username = reader.GetString();
            Password = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Username);
            writer.Put(Password);
        }
    }
}