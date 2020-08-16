using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterBuff(MySqlDataReader reader, out CharacterBuff result)
        {
            if (reader.Read())
            {
                result = new CharacterBuff();
                result.type = (BuffType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                result.level = reader.GetInt16("level");
                result.buffRemainsDuration = reader.GetFloat("buffRemainsDuration");
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public async Task CreateCharacterBuff(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterBuff characterBuff)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterbuff (id, characterId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @level, @buffRemainsDuration)",
                new MySqlParameter("@id", characterId + "_" + characterBuff.type + "_" + characterBuff.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterBuff.type),
                new MySqlParameter("@dataId", characterBuff.dataId),
                new MySqlParameter("@level", characterBuff.level),
                new MySqlParameter("@buffRemainsDuration", characterBuff.buffRemainsDuration));
        }

        public async Task<List<CharacterBuff>> ReadCharacterBuffs(string characterId, List<CharacterBuff> result = null)
        {
            if (result == null)
                result = new List<CharacterBuff>();
            await ExecuteReader((reader) =>
            {
                CharacterBuff tempBuff;
                while (ReadCharacterBuff(reader, out tempBuff))
                {
                    result.Add(tempBuff);
                }
            }, "SELECT * FROM characterbuff WHERE characterId=@characterId ORDER BY buffRemainsDuration ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public async Task DeleteCharacterBuffs(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterbuff WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
