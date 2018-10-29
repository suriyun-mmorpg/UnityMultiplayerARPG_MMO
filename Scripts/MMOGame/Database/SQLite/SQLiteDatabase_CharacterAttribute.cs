using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
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
                result.dataId = reader.GetInt32("dataId");
                result.amount = reader.GetInt16("amount");
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public void CreateCharacterAttribute(string characterId, CharacterAttribute characterAttribute)
        {
            ExecuteNonQuery("INSERT INTO characterattribute (characterId, dataId, amount) VALUES (@characterId, @dataId, @amount)",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterAttribute.dataId),
                new SqliteParameter("@amount", characterAttribute.amount));
        }

        public List<CharacterAttribute> ReadCharacterAttributes(string characterId)
        {
            var result = new List<CharacterAttribute>();
            var reader = ExecuteReader("SELECT * FROM characterattribute WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            CharacterAttribute tempAttribute;
            while (ReadCharacterAttribute(reader, out tempAttribute, false))
            {
                result.Add(tempAttribute);
            }
            return result;
        }

        public void DeleteCharacterAttributes(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterattribute WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
