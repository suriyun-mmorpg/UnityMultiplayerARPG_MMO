using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CreateUserLoginReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Username = reader.GetString();
            Password = reader.GetString();
            Email = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Username);
            writer.Put(Password);
            writer.Put(Email);
        }
    }
}