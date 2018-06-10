using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterAttribute(SQLiteRowsReader reader, out CharacterAttribute result, bool resetReader = true)
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

        public override async Task CreateCharacterAttribute(string characterId, CharacterAttribute characterAttribute)
        {
            var connection = NewConnection();
            connection.Open();
            await CreateCharacterAttribute(connection, characterId, characterAttribute);
            connection.Close();
        }

        public async Task CreateCharacterAttribute(SqliteConnection connection, string characterId, CharacterAttribute characterAttribute)
        {
            await ExecuteNonQuery(connection, "INSERT INTO characterattribute (characterId, attributeId, amount) VALUES (@characterId, @attributeId, @amount)",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@attributeId", characterAttribute.attributeId),
                new SqliteParameter("@amount", characterAttribute.amount));
        }

        public override async Task<CharacterAttribute> ReadCharacterAttribute(string characterId, string attributeId)
        {
            var reader = await ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId AND attributeId=@attributeId LIMIT 1",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@attributeId", attributeId));
            CharacterAttribute result;
            ReadCharacterAttribute(reader, out result);
            return result;
        }

        public override async Task<List<CharacterAttribute>> ReadCharacterAttributes(string characterId)
        {
            var result = new List<CharacterAttribute>();
            var reader = await ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            CharacterAttribute tempAttribute;
            while (ReadCharacterAttribute(reader, out tempAttribute, false))
            {
                result.Add(tempAttribute);
            }
            return result;
        }

        public override async Task UpdateCharacterAttribute(string characterId, CharacterAttribute characterAttribute)
        {
            await ExecuteNonQuery("UPDATE characterattribute SET amount=@amount WHERE characterId=@characterId AND attributeId=@attributeId",
                new SqliteParameter("@amount", characterAttribute.amount),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@attributeId", characterAttribute.attributeId));
        }

        public override async Task DeleteCharacterAttribute(string characterId, string attributeId)
        {
            await ExecuteNonQuery("DELETE FROM characterattribute WHERE characterId=@characterId AND attributeId=@attributeId",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@attributeId", attributeId));
        }
    }
}
