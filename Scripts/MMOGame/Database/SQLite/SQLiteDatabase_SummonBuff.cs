#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void CreateSummonBuff(SqliteTransaction transaction, HashSet<string> insertedIds, string characterId, CharacterBuff summonBuff)
        {
            string id = summonBuff.id;
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Summon buff {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuery(transaction, "INSERT INTO summonbuffs (id, characterId, buffId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @buffId, @type, @dataId, @level, @buffRemainsDuration)",
                new SqliteParameter("@id", id),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@buffId", summonBuff.id),
                new SqliteParameter("@type", (byte)summonBuff.type),
                new SqliteParameter("@dataId", summonBuff.dataId),
                new SqliteParameter("@level", summonBuff.level),
                new SqliteParameter("@buffRemainsDuration", summonBuff.buffRemainsDuration));
        }

        private bool ReadSummonBuff(SqliteDataReader reader, out CharacterBuff result)
        {
            if (reader.Read())
            {
                result = new CharacterBuff();
                result.id = reader.GetString(0);
                result.type = (BuffType)reader.GetByte(1);
                result.dataId = reader.GetInt32(2);
                result.level = reader.GetInt32(3);
                result.buffRemainsDuration = reader.GetFloat(4);
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public void DeleteSummonBuff(SqliteTransaction transaction, string characterId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM summonbuffs WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
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
                DeleteSummonBuff(transaction, characterId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < summonBuffs.Count; ++i)
                {
                    CreateSummonBuff(transaction, insertedIds, characterId, summonBuffs[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing buffs of summon: " + characterId);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
        }
    }
}
#endif
