using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace Insthync.MMOG
{
    public class RequestAppServerRegistrationMessage : ILiteNetLibMessage
    {
        public uint ackId;
        public CentralServerPeerInfo peerInfo;
        public int time { get; private set; }
        public string hash { get; private set; }

        public void Deserialize(NetDataReader reader)
        {
            ackId = reader.GetUInt();
            peerInfo = new CentralServerPeerInfo();
            peerInfo.Deserialize(reader);
            time = reader.GetInt();
            hash = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ackId);
            if (peerInfo == null)
                peerInfo = new CentralServerPeerInfo();
            time = (int)(System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
            hash = CentralNetworkManager.GetAppServerRegistrationHash(peerInfo.peerType, time);
            peerInfo.Serialize(writer);
            writer.Put(time);
            writer.Put(hash);
        }

        public bool ValidateHash()
        {
            if (string.IsNullOrEmpty(hash) || peerInfo == null)
                return false;
            return hash.Equals(CentralNetworkManager.GetAppServerRegistrationHash(peerInfo.peerType, time));
        }
    }
}
