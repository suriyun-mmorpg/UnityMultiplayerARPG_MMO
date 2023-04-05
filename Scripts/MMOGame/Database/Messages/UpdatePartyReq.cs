using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdatePartyReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            PartyId = reader.GetInt();
            ShareExp = reader.GetBool();
            ShareItem = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PartyId);
            writer.Put(ShareExp);
            writer.Put(ShareItem);
        }
    }
}