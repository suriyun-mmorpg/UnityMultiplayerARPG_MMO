using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

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
                result.dataId = reader.GetInt32("dataId");
                result.amount = reader.GetInt32("amount");
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public async Task CreateCharacterAttribute(MySqlConnection connection, string characterId, CharacterAttribute characterAttribute)
        {
            await ExecuteNonQuery(connection, "INSERT INTO characterattribute (id, characterId, dataId, amount) VALUES (@id, @characterId, @dataId, @amount)",
                new MySqlParameter("@id", characterId + "_" + characterAttribute.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterAttribute.dataId),
                new MySqlParameter("@amount", characterAttribute.amount));
        }

        public async Task<List<CharacterAttribute>> ReadCharacterAttributes(string characterId)
        {
            var result = new List<CharacterAttribute>();
            var reader = await ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterAttribute tempAttribute;
            while (ReadCharacterAttribute(reader, out tempAttribute, false))
            {
                result.Add(tempAttribute);
            }
            return result;
        }

        public async Task DeleteCharacterAttributes(MySqlConnection connection, string characterId)
        {
            await ExecuteNonQuery(connection, "DELETE FROM characterattribute WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
