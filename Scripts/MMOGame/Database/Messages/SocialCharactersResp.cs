using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct SocialCharactersResp : INetSerializable
    {
        public List<SocialCharacterData> List { get; set; }

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