#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Cysharp.Text;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterAttribute(MySqlDataReader reader, out CharacterAttribute result)
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

        public void CreateCharacterAttribute(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, string characterId, CharacterAttribute characterAttribute)
        {
            string id = ZString.Concat(characterId, "_", characterAttribute.dataId);
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Attribute {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characterattribute (id, characterId, dataId, amount) VALUES (@id, @characterId, @dataId, @amount)",
                new MySqlParameter("@id", id),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterAttribute.dataId),
                new MySqlParameter("@amount", characterAttribute.amount));
        }

        public List<CharacterAttribute> ReadCharacterAttributes(string characterId, List<CharacterAttribute> result = null)
        {
            if (result == null)
                result = new List<CharacterAttribute>();
            ExecuteReaderSync((reader) =>
            {
                CharacterAttribute tempAttribute;
                while (ReadCharacterAttribute(reader, out tempAttribute))
                {
                    result.Add(tempAttribute);
                }
            }, "SELECT dataId, amount FROM characterattribute WHERE characterId=@characterId ORDER BY id ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterAttributes(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM characterattribute WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif