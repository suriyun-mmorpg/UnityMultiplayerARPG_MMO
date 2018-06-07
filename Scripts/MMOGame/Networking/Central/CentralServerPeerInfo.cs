using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class CentralServerPeerInfo : ILiteNetLibMessage
    {
        public CentralServerPeerType peerType;
        public string networkAddress;
        public int networkPort;
        public string connectKey;
        public string extra;

        public void Deserialize(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
            connectKey = reader.GetString();
            extra = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(networkAddress);
            writer.Put(networkPort);
            writer.Put(connectKey);
            writer.Put(extra);
        }
    }
}
