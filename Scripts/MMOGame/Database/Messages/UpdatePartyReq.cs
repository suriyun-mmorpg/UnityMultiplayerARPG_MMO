using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct UpdatePartyReq : INetSerializable
    {
        public int PartyId { get; set; }
        public bool ShareExp { get; set; }
        public bool ShareItem { get; set; }

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