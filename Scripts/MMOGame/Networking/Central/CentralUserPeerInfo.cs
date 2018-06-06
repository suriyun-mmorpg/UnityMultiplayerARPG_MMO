using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class CentralUserPeerInfo : ILiteNetLibMessage
    {
        public string userId;
        public string selectCharacterId;
        public string currentMap;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            selectCharacterId = reader.GetString();
            currentMap = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(selectCharacterId);
            writer.Put(currentMap);
        }
    }
}
