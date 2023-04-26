using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseCharactersMessage : INetSerializable
    {
        public UITextKeys message;
        public List<PlayerCharacterData> characters;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();

            characters = new List<PlayerCharacterData>();
            byte count = reader.GetByte();
            for (byte i = 0; i < count; ++i)
            {
                PlayerCharacterData character = new PlayerCharacterData();
                characters.Add(character.DeserializeCharacterData(reader, withTransforms: false, withBuffs: false, withSkillUsages: false, withNonEquipItems: false, withSummons: false, withHotkeys: false, withQuests: false));
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (characters == null)
            {
                writer.Put(byte.MinValue);
            }
            else
            {
                writer.Put((byte)characters.Count);
                foreach (PlayerCharacterData character in characters)
                {
                    character.SerializeCharacterData(writer, withTransforms: false, withBuffs: false, withSkillUsages: false, withNonEquipItems: false, withSummons: false, withHotkeys: false, withQuests: false);
                }
            }
        }
    }
}
