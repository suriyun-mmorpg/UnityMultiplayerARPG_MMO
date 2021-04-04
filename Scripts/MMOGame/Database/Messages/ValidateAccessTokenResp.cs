using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ValidateAccessTokenResp : INetSerializable
    {
        public bool IsPass { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            IsPass = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsPass);
        }
    }
}