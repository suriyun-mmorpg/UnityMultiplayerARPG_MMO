using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterBuff(MySQLRowsReader reader, out CharacterBuff result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterBuff();
                result.id = reader.GetString("id");
                result.characterId = reader.GetString("characterId");
                result.type = (BuffType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                result.level = (short)reader.GetInt32("level");
                result.buffRemainsDuration = reader.GetFloat("buffRemainsDuration");
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public async Task CreateCharacterBuff(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterBuff characterBuff)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterbuff (id, characterId, type, dataId, level, buffRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @level, @buffRemainsDuration)",
                new MySqlParameter("@id", characterBuff.id),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterBuff.type),
                new MySqlParameter("@dataId", characterBuff.dataId),
                new MySqlParameter("@level", characterBuff.level),
                new MySqlParameter("@buffRemainsDuration", characterBuff.buffRemainsDuration));
        }
        
        public async Task<List<CharacterBuff>> ReadCharacterBuffs(string characterId)
        {
            var result = new List<CharacterBuff>();
            var reader = await ExecuteReader("SELECT * FROM characterbuff WHERE characterId=@characterId ORDER BY buffRemainsDuration ASC",
                new MySqlParameter("@characterId", characterId));
            CharacterBuff tempBuff;
            while (ReadCharacterBuff(reader, out tempBuff, false))
            {
                result.Add(tempBuff);
            }
            return result;
        }

        public async Task DeleteCharacterBuffs(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterbuff WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
