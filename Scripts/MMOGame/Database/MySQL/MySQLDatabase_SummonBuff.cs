#if UNITY_SERVER || !MMO_BUILD
using LiteNetLibManager;
using MySqlConnector;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadSummonBuff(MySqlDataReader reader, out CharacterBuff result)
        {
            if (reader.Read())
            {
                result = new CharacterBuff();
                result.id = reader.GetString(0);
                result.type = (BuffType)reader.GetByte(1);
                result.dataId = reader.GetInt32(2);
                result.level = reader.GetInt16(3);
                result.buffRemainsDuration = reader.GetFloat(4);
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public override List<CharacterBuff> GetSummonBuffs(string characterId)
        {
            List<CharacterBuff> result = new List<CharacterBuff>();
            ExecuteReaderSync((reader) =>
            {
                CharacterBuff tempBuff;
                while (ReadSummonBuff(reader, out tempBuff))
                {
                    result.Add(tempBuff);
                }
            }, "SELECT buffId, type, dataId, level, buffRemainsDuration FROM summonbuffs WHERE characterId=@characterId ORDER BY buffRemainsDuration ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public override void SetSummonBuffs(string characterId, List<CharacterBuff> summonBuffs)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                ExecuteNonQuerySync(connection, transaction, "DELETE FROM summonbuffs WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
                foreach (CharacterBuff summonBuff in summonBuffs)
                {
                    ExecuteNonQuerySync(connection, transaction, "INSERT INTO summonbuffs (id, characterId, buffId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @buffId, @type, @dataId, @level, @buffRemainsDuration)",
                        new MySqlParameter("@id", characterId + "_" + summonBuff.id),
                        new MySqlParameter("@characterId", characterId),
                        new MySqlParameter("@buffId", summonBuff.id),
                        new MySqlParameter("@type", (byte)summonBuff.type),
                        new MySqlParameter("@dataId", summonBuff.dataId),
                        new MySqlParameter("@level", summonBuff.level),
                        new MySqlParameter("@buffRemainsDuration", summonBuff.buffRemainsDuration));
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing buffs of summon: " + characterId);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }
    }
}
#endif
