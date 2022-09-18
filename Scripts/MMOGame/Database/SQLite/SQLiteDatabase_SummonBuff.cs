#if UNITY_EDITOR || UNITY_SERVER || !MMO_BUILD
using LiteNetLibManager;
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadSummonBuff(SqliteDataReader reader, out CharacterBuff result)
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
            ExecuteReader((reader) =>
            {
                CharacterBuff tempBuff;
                while (ReadSummonBuff(reader, out tempBuff))
                {
                    result.Add(tempBuff);
                }
            }, "SELECT buffId, type, dataId, level, buffRemainsDuration FROM summonbuffs WHERE characterId=@characterId ORDER BY buffRemainsDuration ASC",
                new SqliteParameter("@characterId", characterId));
            return result;
        }

        public override void SetSummonBuffs(string characterId, List<CharacterBuff> summonBuffs)
        {
            SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                ExecuteNonQuery(transaction, "DELETE FROM summonbuffs WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
                foreach (CharacterBuff summonBuff in summonBuffs)
                {
                    ExecuteNonQuery(transaction, "INSERT INTO summonbuffs (id, characterId, buffId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @buffId, @type, @dataId, @level, @buffRemainsDuration)",
                        new SqliteParameter("@id", characterId + "_" + summonBuff.id),
                        new SqliteParameter("@characterId", characterId),
                        new SqliteParameter("@buffId", summonBuff.id),
                        new SqliteParameter("@type", (byte)summonBuff.type),
                        new SqliteParameter("@dataId", summonBuff.dataId),
                        new SqliteParameter("@level", summonBuff.level),
                        new SqliteParameter("@buffRemainsDuration", summonBuff.buffRemainsDuration));
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
        }
    }
}
#endif
