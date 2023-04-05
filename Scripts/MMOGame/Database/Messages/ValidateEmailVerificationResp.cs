using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ValidateEmailVerificationResp : INetSerializable
    {
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