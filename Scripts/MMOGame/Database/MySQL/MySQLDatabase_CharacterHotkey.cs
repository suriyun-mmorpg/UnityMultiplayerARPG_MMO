#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        public async UniTask CreateCharacterHotkey(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterHotkey characterHotkey)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterhotkey (id, characterId, hotkeyId, type, relateId) VALUES (@id, @characterId, @hotkeyId, @type, @relateId)",
                new MySqlParameter("@id", characterId + "_" + characterHotkey.hotkeyId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", characterHotkey.hotkeyId),
                new MySqlParameter("@type", characterHotkey.type),
                new MySqlParameter("@relateId", characterHotkey.relateId));
        }

        public async UniTask<List<CharacterHotkey>> ReadCharacterHotkeys(string characterId, List<CharacterHotkey> result = null)
        {
            if (result == null)
                result = new List<CharacterHotkey>();
            await ExecuteReader((reader) =>
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

        public async UniTask DeleteCharacterHotkeys(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterhotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif