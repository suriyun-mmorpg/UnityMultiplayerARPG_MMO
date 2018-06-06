using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterAttribute(MySQLRowsReader reader, out CharacterAttribute result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterAttribute();
                result.attributeId = reader.GetString("attributeId");
                result.amount = reader.GetInt32("amount");
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public override void CreateCharacterAttribute(string characterId, CharacterAttribute characterAttribute)
        {
            ExecuteNonQuery("INSERT INTO characterattribute (characterId, attributeId, amount) VALUES (@characterId, @attributeId, @amount)",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@attributeId", characterAttribute.attributeId),
                new MySqlParameter("@amount", characterAttribute.amount));
        }

        public override CharacterAttribute ReadCharacterAttribute(string characterId, string attributeId)
        {
            var reader = ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId AND attributeId=@attributeId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@attributeId", attributeId));
            CharacterAttribute result;
            ReadCharacterAttribute(reader, out result);
            return result;
        }

        public override List<CharacterAttribute> ReadCharacterAttributes(string characterId)
        {
            var result = new List<CharacterAttribute>();
            var reader = ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterAttribute tempAttribute;
            while (ReadCharacterAttribute(reader, out tempAttribute, false))
            {
                result.Add(tempAttribute);
            }
            return result;
        }

        public override void UpdateCharacterAttribute(string characterId, CharacterAttribute characterAttribute)
        {
            ExecuteNonQuery("UPDATE characterattribute SET amount=@amount WHERE characterId=@characterId AND attributeId=@attributeId",
                new MySqlParameter("@amount", characterAttribute.amount),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@attributeId", characterAttribute.attributeId));
        }

        public override void DeleteCharacterAttribute(string characterId, string attributeId)
        {
            ExecuteNonQuery("DELETE FROM characterattribute WHERE characterId=@characterId AND attributeId=@attributeId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@attributeId", attributeId));
        }
    }
}
