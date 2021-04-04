using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public struct BuildingsResp : INetSerializable
    {
        public List<BuildingSaveData> List { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<BuildingSaveData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}