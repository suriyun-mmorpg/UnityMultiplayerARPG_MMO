using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ValidateUserLoginResp : INetSerializable
    {
        public string UserId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
        }
    }
}