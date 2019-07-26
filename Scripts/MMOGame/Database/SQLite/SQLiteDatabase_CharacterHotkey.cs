using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
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
                result.relateId = reader.GetString("relateId");
                return true;
            }
            result = CharacterHotkey.Empty;
            return false;
        }

        public void CreateCharacterHotkey(string characterId, CharacterHotkey characterHotkey)
        {
            ExecuteNonQuery("INSERT INTO characterhotkey (id, characterId, hotkeyId, type, dataId) VALUES (@id, @characterId, @hotkeyId, @type, @dataId)",
                new SqliteParameter("@id", characterId + "_" + characterHotkey.hotkeyId),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@hotkeyId", characterHotkey.hotkeyId),
                new SqliteParameter("@type", characterHotkey.type),
                new SqliteParameter("@relateId", characterHotkey.relateId));
        }

        public List<CharacterHotkey> ReadCharacterHotkeys(string characterId)
        {
            List<CharacterHotkey> result = new List<CharacterHotkey>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM characterhotkey WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            CharacterHotkey tempHotkey;
            while (ReadCharacterHotkey(reader, out tempHotkey, false))
            {
                result.Add(tempHotkey);
            }
            return result;
        }

        public void DeleteCharacterHotkeys(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterhotkey WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
