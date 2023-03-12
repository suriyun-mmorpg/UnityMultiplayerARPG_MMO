#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Cysharp.Text;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterHotkey(MySqlDataReader reader, out CharacterHotkey result)
        {
            if (reader.Read())
            {
                result = new CharacterHotkey();
                result.hotkeyId = reader.GetString(0);
                result.type = (HotkeyType)reader.GetByte(1);
                result.relateId = reader.GetString(2);
                return true;
            }
            result = CharacterHotkey.Empty;
            return false;
        }

        public void CreateCharacterHotkey(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, string characterId, CharacterHotkey characterHotkey)
        {
            string id = ZString.Concat(characterId, "_", characterHotkey.hotkeyId);
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Hotkey {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characterhotkey (id, characterId, hotkeyId, type, relateId) VALUES (@id, @characterId, @hotkeyId, @type, @relateId)",
                new MySqlParameter("@id", id),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId),
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@relateId", characterHotkey.relateId));
        }

        public List<CharacterHotkey> ReadCharacterHotkeys(string characterId, List<CharacterHotkey> result = null)
        {
            if (result == null)
                result = new List<CharacterHotkey>();
            ExecuteReaderSync((reader) =>
            {
                CharacterHotkey tempHotkey;
                while (ReadCharacterHotkey(reader, out tempHotkey))
                {
                    result.Add(tempHotkey);
                }
            }, "SELECT hotkeyId, type, relateId FROM characterhotkey WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterHotkeys(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM characterhotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif