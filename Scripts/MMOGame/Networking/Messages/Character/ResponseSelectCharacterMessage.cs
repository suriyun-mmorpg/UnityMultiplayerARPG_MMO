using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseSelectCharacterMessage : INetSerializable
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

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            sceneName = reader.GetString();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(sceneName);
            writer.Put(networkAddress);
            writer.Put(networkPort);
        }
    }
}
