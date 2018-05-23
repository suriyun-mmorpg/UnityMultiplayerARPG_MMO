using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

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

        public override void CreateCharacterHotkey(string characterId, CharacterHotkey characterHotkey)
        {
            ExecuteNonQuery("INSERT INTO characterHotkey (characterId, hotkeyId, type, dataId) VALUES (@characterId, @hotkeyId, @type, @dataId)",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId),
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@dataId", characterHotkey.dataId));
        }

        public override CharacterHotkey ReadCharacterHotkey(string characterId, string hotkeyId)
        {
            var reader = ExecuteReader("SELECT * FROM characterHotkey WHERE characterId=@characterId AND hotkeyId=@hotkeyId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", hotkeyId));
            CharacterHotkey result;
            ReadCharacterHotkey(reader, out result);
            return result;
        }

        public override List<CharacterHotkey> ReadCharacterHotkeys(string characterId)
        {
            var result = new List<CharacterHotkey>();
            var reader = ExecuteReader("SELECT * FROM characterHotkey WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterHotkey tempHotkey;
            while (ReadCharacterHotkey(reader, out tempHotkey, false))
            {
                result.Add(tempHotkey);
            }
            return result;
        }

        public override void UpdateCharacterHotkey(string characterId, CharacterHotkey characterHotkey)
        {
            ExecuteNonQuery("UPDATE characterHotkey SET type=@type, dataId=@dataId WHERE characterId=@characterId AND hotkeyId=@hotkeyId",
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@dataId", characterHotkey.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId));
        }

        public override void DeleteCharacterHotkey(string characterId, string hotkeyId)
        {
            ExecuteNonQuery("DELETE FROM characterHotkey WHERE characterId=@characterId AND hotkeyId=@hotkeyId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", hotkeyId));
        }
    }
}
