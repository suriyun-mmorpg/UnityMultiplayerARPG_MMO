using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindUsernameReq : INetSerializable
    {
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