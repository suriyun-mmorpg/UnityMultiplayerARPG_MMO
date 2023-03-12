#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Cysharp.Text;
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
                result.amount = reader.GetInt32(1);
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public void CreateCharacterAttribute(SqliteTransaction transaction, HashSet<string> insertedIds, int idx, string characterId, CharacterAttribute characterAttribute)
        {
            string id = ZString.Concat(characterId, "_", characterAttribute.dataId);
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Attribute {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuery(transaction, "INSERT INTO characterattribute (id, idx, characterId, dataId, amount) VALUES (@id, @idx, @characterId, @dataId, @amount)",
                new SqliteParameter("@id", id),
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
            }, "SELECT dataId, amount FROM characterattribute WHERE characterId=@characterId ORDER BY id ASC",
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