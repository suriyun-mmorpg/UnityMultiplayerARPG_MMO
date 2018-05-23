using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
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
                result.characterId = reader.GetInt64("characterId").ToString();
                result.dataId = reader.GetString("dataId");
                result.type = (BuffType)reader.GetByte("type");
                result.level = reader.GetInt32("level");
                result.buffRemainsDuration = reader.GetFloat("buffRemainsDuration");
                return true;
            }
            result = CharacterBuff.Empty;
            return false;
        }

        public override CharacterBuff ReadCharacterBuff(string id)
        {
            var reader = ExecuteReader("SELECT * FROM characterBuff WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            CharacterBuff result;
            ReadCharacterBuff(reader, out result);
            return result;
        }

        public override List<CharacterBuff> ReadCharacterBuffs(string characterId)
        {
            var result = new List<CharacterBuff>();
            var reader = ExecuteReader("SELECT * FROM characterBuff WHERE applyingCharacterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterBuff tempBuff;
            while (ReadCharacterBuff(reader, out tempBuff, false))
            {
                result.Add(tempBuff);
            }
            return result;
        }
    }
}
