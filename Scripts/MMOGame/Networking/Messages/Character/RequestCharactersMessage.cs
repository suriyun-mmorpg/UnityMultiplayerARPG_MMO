using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class RequestCharactersMessage : BaseAckMessage
    {
        public override void DeserializeData(NetDataReader reader)
        {
        }

        public override void SerializeData(NetDataWriter writer)
        {
        }
    }
}
