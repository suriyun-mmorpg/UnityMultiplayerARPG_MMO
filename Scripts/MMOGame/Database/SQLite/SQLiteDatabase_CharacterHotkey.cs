#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterHotkey(SqliteDataReader reader, out CharacterHotkey result)
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

        public void CreateCharacterHotkey(SqliteTransaction transaction, string characterId, CharacterHotkey characterHotkey)
        {
            ExecuteNonQuery(transaction, "INSERT INTO characterhotkey (id, characterId, hotkeyId, type, relateId) VALUES (@id, @characterId, @hotkeyId, @type, @relateId)",
                new SqliteParameter("@id", characterId + "_" + characterHotkey.hotkeyId),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@hotkeyId", characterHotkey.hotkeyId),
                new SqliteParameter("@type", characterHotkey.type),
                new SqliteParameter("@relateId", characterHotkey.relateId));
        }

        public List<CharacterHotkey> ReadCharacterHotkeys(string characterId)
        {
            List<CharacterHotkey> result = new List<CharacterHotkey>();
            ExecuteReader((reader) =>
            {
                CharacterHotkey tempHotkey;
                while (ReadCharacterHotkey(reader, out tempHotkey))
                {
                    result.Add(tempHotkey);
                }
            }, "SELECT hotkeyId, type, relateId FROM characterhotkey WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterHotkeys(SqliteTransaction transaction, string characterId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM characterhotkey WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
#endif