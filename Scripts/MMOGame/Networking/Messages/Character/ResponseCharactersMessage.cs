using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public class ResponseCharactersMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotLoggedin,
        }
        public Error error;
        public List<PlayerCharacterData> characters;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();

            characters = new List<PlayerCharacterData>();
            int count = reader.GetInt();
            for (int i = 0; i < count; ++i)
            {
                PlayerCharacterData character = new PlayerCharacterData();
                characters.Add(character.DeserializeCharacterData(reader));
            }
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (characters == null)
                characters = new List<PlayerCharacterData>();
            writer.Put(characters.Count);
            foreach (PlayerCharacterData character in characters)
            {
                character.SerializeCharacterData(writer);
            }
        }
    }
}
