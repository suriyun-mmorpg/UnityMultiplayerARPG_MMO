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
                result.type = (HotkeyType)reader.GetByte("type");
                result.dataId = reader.GetString("dataId");
                return true;
            }
            result = CharacterHotkey.Empty;
            return false;
        }

        public override async Task CreateCharacterHotkey(string characterId, CharacterHotkey characterHotkey)
        {
            await ExecuteNonQuery("INSERT INTO characterhotkey (characterId, hotkeyId, type, dataId) VALUES (@characterId, @hotkeyId, @type, @dataId)",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId),
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@dataId", characterHotkey.dataId));
        }

        public override async Task<CharacterHotkey> ReadCharacterHotkey(string characterId, string hotkeyId)
        {
            var reader = await ExecuteReader("SELECT * FROM characterhotkey WHERE characterId=@characterId AND hotkeyId=@hotkeyId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", hotkeyId));
            CharacterHotkey result;
            ReadCharacterHotkey(reader, out result);
            return result;
        }

        public override async Task<List<CharacterHotkey>> ReadCharacterHotkeys(string characterId)
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

        public override async Task UpdateCharacterHotkey(string characterId, CharacterHotkey characterHotkey)
        {
            await ExecuteNonQuery("UPDATE characterhotkey SET type=@type, dataId=@dataId WHERE characterId=@characterId AND hotkeyId=@hotkeyId",
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@dataId", characterHotkey.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId));
        }

        public override async Task DeleteCharacterHotkey(string characterId, string hotkeyId)
        {
            await ExecuteNonQuery("DELETE FROM characterhotkey WHERE characterId=@characterId AND hotkeyId=@hotkeyId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", hotkeyId));
        }
    }
}
