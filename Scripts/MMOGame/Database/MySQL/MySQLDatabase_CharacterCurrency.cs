#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterCurrency(MySqlDataReader reader, out CharacterCurrency result)
        {
            if (reader.Read())
            {
                result = new CharacterCurrency();
                result.dataId = reader.GetInt32(0);
                result.amount = reader.GetInt32(1);
                return true;
            }
            result = CharacterCurrency.Empty;
            return false;
        }

        public void CreateCharacterCurrency(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterCurrency characterCurrency)
        {
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO charactercurrency (id, characterId, dataId, amount) VALUES (@id, @characterId, @dataId, @amount)",
                new MySqlParameter("@id", characterId + "_" + characterCurrency.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterCurrency.dataId),
                new MySqlParameter("@amount", characterCurrency.amount));
        }

        public List<CharacterCurrency> ReadCharacterCurrencies(string characterId, List<CharacterCurrency> result = null)
        {
            if (result == null)
                result = new List<CharacterCurrency>();
            ExecuteReaderSync((reader) =>
            {
                CharacterCurrency tempCurrency;
                while (ReadCharacterCurrency(reader, out tempCurrency))
                {
                    result.Add(tempCurrency);
                }
            }, "SELECT dataId, amount FROM charactercurrency WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterCurrencies(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM charactercurrency WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif