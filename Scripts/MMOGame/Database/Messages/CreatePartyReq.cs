using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CreatePartyReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            ShareExp = reader.GetBool();
            ShareItem = reader.GetBool();
            LeaderCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ShareExp);
            writer.Put(ShareItem);
            writer.Put(LeaderCharacterId);
        }
    }
}