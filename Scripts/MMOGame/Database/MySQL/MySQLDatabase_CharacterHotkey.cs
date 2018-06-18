using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterHotkey(MySQLRowsReader reader, out CharacterHotkey result, bool resetReader = true)
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

        public async Task CreateCharacterHotkey(MySqlConnection connection, string characterId, CharacterHotkey characterHotkey)
        {
            await ExecuteNonQuery(connection, "INSERT INTO characterhotkey (id, characterId, hotkeyId, type, dataId) VALUES (@id, @characterId, @hotkeyId, @type, @dataId)",
                new MySqlParameter("@id", characterId + "_" + characterHotkey.hotkeyId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId),
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@dataId", characterHotkey.dataId));
        }

        public async Task<List<CharacterHotkey>> ReadCharacterHotkeys(string characterId)
        {
            var result = new List<CharacterHotkey>();
            var reader = await ExecuteReader("SELECT * FROM characterhotkey WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterHotkey tempHotkey;
            while (ReadCharacterHotkey(reader, out tempHotkey, false))
            {
                result.Add(tempHotkey);
            }
            return result;
        }

        public async Task DeleteCharacterHotkeys(MySqlConnection connection, string characterId)
        {
            await ExecuteNonQuery(connection, "DELETE FROM characterhotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
