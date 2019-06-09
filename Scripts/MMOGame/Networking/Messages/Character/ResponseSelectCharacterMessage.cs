using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseSelectCharacterMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotLoggedin,
            AlreadySelectCharacter,
            InvalidCharacterData,
            MapNotReady,
        }
        public Error error;
        public string sceneName;
        public string networkAddress;
        public int networkPort;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            sceneName = reader.GetString();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(sceneName);
            writer.Put(networkAddress);
            writer.Put(networkPort);
        }
    }
}
