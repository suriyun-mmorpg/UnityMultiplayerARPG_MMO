using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {

        public override CharacterBuff ReadCharacterBuff(string id)
        {
            var reader = ExecuteReader("SELECT characterId, dataId, type, level, buffRemainsDuration FROM characterBuff WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            if (reader.Read())
            {
                var result = new CharacterBuff();
                result.characterId = reader.GetInt64(0).ToString();
                result.dataId = reader.GetString(1);
                result.type = (BuffType)reader.GetByte(2);
                result.level = reader.GetInt32(3);
                result.buffRemainsDuration = reader.GetFloat(4);
                return result;
            }
            return CharacterBuff.Empty;
        }

        public override List<CharacterBuff> ReadCharacterBuffs(string characterId)
        {
            throw new System.NotImplementedException();
        }
    }
}
