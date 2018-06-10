using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Threading.Tasks;

namespace Insthync.MMOG
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
                result.id = reader.GetString("id");
                result.characterId = reader.GetInt64("characterId").ToString();
                result.type = (BuffType)reader.GetByte("type");
                result.dataId = reader.GetString("dataId");
                result.level = reader.GetInt32("level");
                result.buffRemainsDuration = reader.GetFloat("buffRemainsDuration");
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public override async Task CreateCharacterBuff(string characterId, CharacterBuff characterBuff)
        {
            await ExecuteNonQuery("INSERT INTO characterbuff (id, characterId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @level, @buffRemainsDuration)",
                new SqliteParameter("@id", characterBuff.id),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@type", (byte)characterBuff.type),
                new SqliteParameter("@dataId", characterBuff.dataId),
                new SqliteParameter("@level", characterBuff.level),
                new SqliteParameter("@buffRemainsDuration", characterBuff.buffRemainsDuration));
        }

        public override async Task<CharacterBuff> ReadCharacterBuff(string characterId, string id)
        {
            var reader = await ExecuteReader("SELECT * FROM characterbuff WHERE id=@id AND characterId=@characterId LIMIT 1",
                new SqliteParameter("@id", id),
                new SqliteParameter("@characterId", characterId));
            CharacterBuff result;
            ReadCharacterBuff(reader, out result);
            return result;
        }

        public override async Task<List<CharacterBuff>> ReadCharacterBuffs(string characterId)
        {
            var result = new List<CharacterBuff>();
            var reader = await ExecuteReader("SELECT * FROM characterbuff WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            CharacterBuff tempBuff;
            while (ReadCharacterBuff(reader, out tempBuff, false))
            {
                result.Add(tempBuff);
            }
            return result;
        }

        public override async Task UpdateCharacterBuff(string characterId, CharacterBuff characterBuff)
        {
            await ExecuteNonQuery("UPDATE characterbuff SET type=@type, dataId=@dataId, level=@level, buffRemainsDuration=@buffRemainsDuration WHERE id=@id AND characterId=@characterId",
                new SqliteParameter("@type", (byte)characterBuff.type),
                new SqliteParameter("@dataId", characterBuff.dataId),
                new SqliteParameter("@level", characterBuff.level),
                new SqliteParameter("@buffRemainsDuration", characterBuff.buffRemainsDuration),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@id", characterBuff.id));
        }

        public override async Task DeleteCharacterBuff(string characterId, string id)
        {
            await ExecuteNonQuery("DELETE FROM characterbuff WHERE id=@id AND characterId=@characterId",
                new SqliteParameter("@id", id),
                new SqliteParameter("@characterId", characterId));
        }
    }
}
