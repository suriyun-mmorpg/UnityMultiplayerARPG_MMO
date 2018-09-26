using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace MultiplayerARPG.MMO
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
                result.dataId = reader.GetInt32("dataId");
                result.amount = (short)reader.GetInt32("amount");
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public void CreateCharacterAttribute(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterAttribute characterAttribute)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO characterattribute (id, idx, characterId, dataId, amount) VALUES (@id, @idx, @characterId, @dataId, @amount)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterAttribute.dataId),
                new MySqlParameter("@amount", characterAttribute.amount));
        }

        public List<CharacterAttribute> ReadCharacterAttributes(string characterId)
        {
            var result = new List<CharacterAttribute>();
            var reader = ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            CharacterAttribute tempAttribute;
            while (ReadCharacterAttribute(reader, out tempAttribute, false))
            {
                result.Add(tempAttribute);
            }
            return result;
        }

        public void DeleteCharacterAttributes(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM characterattribute WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
