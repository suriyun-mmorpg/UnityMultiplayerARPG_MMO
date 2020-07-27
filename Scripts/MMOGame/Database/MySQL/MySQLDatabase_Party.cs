using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override int CreateParty(bool shareExp, bool shareItem, string leaderId)
        {
            int id = 0;
            MySQLRowsReader reader = ExecuteReader("INSERT INTO party (shareExp, shareItem, leaderId) VALUES (@shareExp, @shareItem, @leaderId);" +
                "SELECT LAST_INSERT_ID();",
                new MySqlParameter("@shareExp", shareExp),
                new MySqlParameter("@shareItem", shareItem),
                new MySqlParameter("@leaderId", leaderId));
            if (reader.Read())
                id = (int)reader.GetUInt64(0);
            if (id > 0)
                ExecuteNonQuery("UPDATE characters SET partyId=@id WHERE id=@leaderId",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@leaderId", leaderId));
            return id;
        }

        public override PartyData ReadParty(int id)
        {
            PartyData result = null;
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM party WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            if (reader.Read())
            {
                result = new PartyData(id, reader.GetBoolean("shareExp"), reader.GetBoolean("shareItem"), reader.GetString("leaderId"));
                reader = ExecuteReader("SELECT id, dataId, characterName, level FROM characters WHERE partyId=@id",
                    new MySqlParameter("@id", id));
                SocialCharacterData partyMemberData;
                while (reader.Read())
                {
                    // Get some required data, other data will be set at server side
                    partyMemberData = new SocialCharacterData();
                    partyMemberData.id = reader.GetString("id");
                    partyMemberData.characterName = reader.GetString("characterName");
                    partyMemberData.dataId = reader.GetInt32("dataId");
                    partyMemberData.level = reader.GetInt16("level");
                    result.AddMember(partyMemberData);
                }
            }
            return result;
        }

        public override void UpdatePartyLeader(int id, string leaderId)
        {
            ExecuteNonQuery("UPDATE party SET leaderId=@leaderId WHERE id=@id",
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@id", id));
        }

        public override void UpdateParty(int id, bool shareExp, bool shareItem)
        {
            ExecuteNonQuery("UPDATE party SET shareExp=@shareExp, shareItem=@shareItem WHERE id=@id",
                new MySqlParameter("@shareExp", shareExp),
                new MySqlParameter("@shareItem", shareItem),
                new MySqlParameter("@id", id));
        }

        public override void DeleteParty(int id)
        {
            ExecuteNonQuery("DELETE FROM party WHERE id=@id;" +
                "UPDATE characters SET partyId=0 WHERE partyId=@id;",
                new MySqlParameter("@id", id));
        }

        public override void UpdateCharacterParty(string characterId, int partyId)
        {
            ExecuteNonQuery("UPDATE characters SET partyId=@partyId WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@partyId", partyId));
        }
    }
}
