using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ValidateUserLoginReq : INetSerializable
    {
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