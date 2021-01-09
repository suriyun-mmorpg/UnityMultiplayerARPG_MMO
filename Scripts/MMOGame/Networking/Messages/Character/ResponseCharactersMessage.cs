using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseCharactersMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotLoggedin,
        }
        public Error error;
        public List<PlayerCharacterData> characters;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();

            characters = new List<PlayerCharacterData>();
            byte count = reader.GetByte();
            for (byte i = 0; i < count; ++i)
            {
                PlayerCharacterData character = new PlayerCharacterData();
                characters.Add(character.DeserializeCharacterData(reader, true));
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (characters == null)
            {
                writer.Put((byte)0);
            }
            else
            {
                writer.Put((byte)characters.Count);
                foreach (PlayerCharacterData character in characters)
                {
                    character.SerializeCharacterData(writer, true);
                }
            }
        }
    }
}
