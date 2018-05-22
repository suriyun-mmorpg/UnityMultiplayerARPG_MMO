using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {

        public override CharacterAttribute ReadCharacterAttribute(string characterId, string attributeId)
        {
            var reader = ExecuteReader("SELECT amount FROM characterAttribute WHERE characterId=@characterId AND attributeId=@attributeId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@attributeId", attributeId));
            if (reader.Read())
            {
                var result = new CharacterAttribute();
                result.attributeId = attributeId;
                result.amount = reader.GetInt32(0);
                return result;
            }
            return CharacterAttribute.Empty;
        }

        public override List<CharacterAttribute> ReadCharacterAttributes(string characterId)
        {
            throw new System.NotImplementedException();
        }
    }
}
