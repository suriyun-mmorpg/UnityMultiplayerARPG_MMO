#if UNITY_EDITOR || UNITY_SERVER || !MMO_BUILD
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterBuff(SqliteDataReader reader, out CharacterBuff result)
        {
            if (reader.Read())
            {
                result = new CharacterBuff();
                result.id = GenericUtils.GetUniqueId();
                result.type = (BuffType)reader.GetByte(0);
                result.dataId = reader.GetInt32(1);
                result.level = reader.GetInt16(2);
                result.buffRemainsDuration = reader.GetFloat(3);
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public void CreateCharacterBuff(SqliteTransaction transaction, string characterId, CharacterBuff characterBuff)
        {
            ExecuteNonQuery(transaction, "INSERT INTO characterbuff (id, characterId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @level, @buffRemainsDuration)",
                new SqliteParameter("@id", characterId + "_" + characterBuff.type + "_" + characterBuff.dataId),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@type", (byte)characterBuff.type),
                new SqliteParameter("@dataId", characterBuff.dataId),
                new SqliteParameter("@level", characterBuff.level),
                new SqliteParameter("@buffRemainsDuration", characterBuff.buffRemainsDuration));
        }

        public List<CharacterBuff> ReadCharacterBuffs(string characterId)
        {
            List<CharacterBuff> result = new List<CharacterBuff>();
            ExecuteReader((reader) =>
            {
                CharacterBuff tempBuff;
                while (ReadCharacterBuff(reader, out tempBuff))
                {
                    result.Add(tempBuff);
                }
            }, "SELECT type, dataId, level, buffRemainsDuration FROM characterbuff WHERE characterId=@characterId ORDER BY buffRemainsDuration ASC",
                new SqliteParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterBuffs(SqliteTransaction transaction, string characterId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM characterbuff WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
#endif