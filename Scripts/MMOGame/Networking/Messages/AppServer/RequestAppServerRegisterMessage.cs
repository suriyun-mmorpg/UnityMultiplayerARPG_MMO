using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestAppServerRegisterMessage : BaseAckMessage
    {
        public CentralServerPeerInfo peerInfo;
        public int time { get; private set; }
        public string hash { get; private set; }

        public override void DeserializeData(NetDataReader reader)
        {
            peerInfo.Deserialize(reader);
            time = reader.GetInt();
            hash = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            time = (int)(System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
            hash = CentralNetworkManager.GetAppServerRegisterHash(peerInfo.peerType, time);
            peerInfo.Serialize(writer);
            writer.Put(time);
            writer.Put(hash);
        }

        public bool ValidateHash()
        {
            if (string.IsNullOrEmpty(hash))
                return false;
            return hash.Equals(CentralNetworkManager.GetAppServerRegisterHash(peerInfo.peerType, time));
        }
    }
}
