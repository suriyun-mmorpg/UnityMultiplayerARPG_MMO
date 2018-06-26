using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class CentralUserPeerInfo : ILiteNetLibMessage
    {
        public NetPeer peer;
        public string userId;
        public string accessToken;
        public string selectCharacterId;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            accessToken = reader.GetString();
            selectCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(accessToken);
            writer.Put(selectCharacterId);
        }
    }
}
