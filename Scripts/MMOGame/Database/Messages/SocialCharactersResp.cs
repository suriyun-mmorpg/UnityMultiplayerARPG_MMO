using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct SocialCharactersResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}