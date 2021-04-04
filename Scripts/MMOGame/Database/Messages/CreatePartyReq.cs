using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct CreatePartyReq : INetSerializable
    {
        public bool ShareExp { get; set; }
        public bool ShareItem { get; set; }
        public string LeaderCharacterId { get; set; }

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