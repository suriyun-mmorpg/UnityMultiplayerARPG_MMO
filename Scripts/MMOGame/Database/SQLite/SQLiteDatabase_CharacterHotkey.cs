using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterHotkey(SQLiteRowsReader reader, out CharacterHotkey result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterHotkey();
                result.hotkeyId = reader.GetString("hotkeyId");
                result.type = (HotkeyType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                return true;
            }
            result = CharacterHotkey.Empty;
            return false;
        }

        public async Task CreateCharacterHotkey(string characterId, CharacterHotkey characterHotkey)
        {
            await ExecuteNonQuery("INSERT INTO characterhotkey (id, characterId, hotkeyId, type, dataId) VALUES (@id, @characterId, @hotkeyId, @type, @dataId)",
                new SqliteParameter("@id", characterId + "_" + characterHotkey.hotkeyId),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@hotkeyId", characterHotkey.hotkeyId),
                new SqliteParameter("@type", characterHotkey.type),
                new SqliteParameter("@dataId", characterHotkey.dataId));
        }

        public async Task<List<CharacterHotkey>> ReadCharacterHotkeys(string characterId)
        {
            var result = new List<CharacterHotkey>();
            var reader = await ExecuteReader("SELECT * FROM characterhotkey WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            CharacterHotkey tempHotkey;
            while (ReadCharacterHotkey(reader, out tempHotkey, false))
            {
                result.Add(tempHotkey);
            }
            return result;
        }

        public async Task DeleteCharacterHotkeys(string characterId)
        {
            await ExecuteNonQuery("DELETE FROM characterhotkey WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
