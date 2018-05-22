using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {

        public override CharacterHotkey ReadCharacterHotkey(string characterId, string hotkeyId)
        {
            var reader = ExecuteReader("SELECT type, dataId FROM characterHotkey WHERE characterId=@characterId AND hotkeyId=@hotkeyId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", hotkeyId));
            if (reader.Read())
            {
                var result = new CharacterHotkey();
                result.hotkeyId = hotkeyId;
                result.type = (HotkeyType)reader.GetByte(0);
                result.dataId = reader.GetString(1);
                return result;
            }
            return CharacterHotkey.Empty;
        }

        public override List<CharacterHotkey> ReadCharacterHotkeys(string characterId)
        {
            var result = new List<CharacterHotkey>();
            var reader = ExecuteReader("SELECT hotkeyId FROM characterHotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            while (reader.Read())
            {
                var hotkeyId = reader.GetString(0);
                result.Add(ReadCharacterHotkey(characterId, hotkeyId));
            }
            return result;
        }
    }
}
