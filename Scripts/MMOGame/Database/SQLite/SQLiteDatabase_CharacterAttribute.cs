#if UNITY_EDITOR || UNITY_SERVER
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterAttribute(SqliteDataReader reader, out CharacterAttribute result)
        {
            if (reader.Read())
            {
                result = new CharacterAttribute();
                result.dataId = reader.GetInt32(0);
                result.amount = reader.GetInt16(1);
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public void CreateCharacterAttribute(SqliteTransaction transaction, int idx, string characterId, CharacterAttribute characterAttribute)
        {
            ExecuteNonQuery(transaction, "INSERT INTO characterattribute (id, idx, characterId, dataId, amount) VALUES (@id, @idx, @characterId, @dataId, @amount)",
                new SqliteParameter("@id", characterId + "_" + idx),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterAttribute.dataId),
                new SqliteParameter("@amount", characterAttribute.amount));
        }

        public List<CharacterAttribute> ReadCharacterAttributes(string characterId)
        {
            List<CharacterAttribute> result = new List<CharacterAttribute>();
            ExecuteReader((reader) =>
            {
                CharacterAttribute tempAttribute;
                while (ReadCharacterAttribute(reader, out tempAttribute))
                {
                    result.Add(tempAttribute);
                }
            }, "SELECT dataId, amount FROM characterattribute WHERE characterId=@characterId ORDER BY idx ASC",
                new SqliteParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterAttributes(SqliteTransaction transaction, string characterId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM characterattribute WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
#endif