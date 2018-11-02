using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterBuff(SQLiteRowsReader reader, out CharacterBuff result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

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

        public void CreateCharacterBuff(string characterId, CharacterBuff characterBuff)
        {
            ExecuteNonQuery("INSERT INTO characterbuff (id, characterId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @level, @buffRemainsDuration)",
                new SqliteParameter("@id", characterId + "_" + characterBuff.type + "_" + characterBuff.dataId),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@type", (byte)characterBuff.type),
                new SqliteParameter("@dataId", characterBuff.dataId),
                new SqliteParameter("@level", characterBuff.level),
                new SqliteParameter("@buffRemainsDuration", characterBuff.buffRemainsDuration));
        }

        public List<CharacterBuff> ReadCharacterBuffs(string characterId)
        {
            var result = new List<CharacterBuff>();
            var reader = ExecuteReader("SELECT * FROM characterbuff WHERE characterId=@characterId ORDER BY buffRemainsDuration ASC",
                new SqliteParameter("@characterId", characterId));
            CharacterBuff tempBuff;
            while (ReadCharacterBuff(reader, out tempBuff, false))
            {
                result.Add(tempBuff);
            }
            return result;
        }

        public void DeleteCharacterBuffs(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterbuff WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
