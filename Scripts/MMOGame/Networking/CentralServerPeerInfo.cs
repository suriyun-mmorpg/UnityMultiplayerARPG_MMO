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
        public string machineAddress;
        public int port;
        public string extra;

        public void Deserialize(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            machineAddress = reader.GetString();
            port = reader.GetInt();
            extra = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(machineAddress);
            writer.Put(port);
            writer.Put(extra);
        }
    }
}
