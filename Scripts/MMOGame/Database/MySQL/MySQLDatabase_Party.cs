using MySqlConnector;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override async Task<int> CreateParty(bool shareExp, bool shareItem, string leaderId)
        {
            int id = 0;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    id = reader.GetInt32(0);
            }, "INSERT INTO party (shareExp, shareItem, leaderId) VALUES (@shareExp, @shareItem, @leaderId);" +
                "SELECT LAST_INSERT_ID();",
                new MySqlParameter("@shareExp", shareExp),
                new MySqlParameter("@shareItem", shareItem),
                new MySqlParameter("@leaderId", leaderId));
            if (id > 0)
                await ExecuteNonQuery("UPDATE characters SET partyId=@id WHERE id=@leaderId",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@leaderId", leaderId));
            return id;
        }

        public override async Task<PartyData> ReadParty(int id)
        {
            PartyData result = null;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    result = new PartyData(id,
                        reader.GetBoolean("shareExp"),
                        reader.GetBoolean("shareItem"),
                        reader.GetString("leaderId"));
                }
            }, "SELECT * FROM party WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));

            await ExecuteReader((reader) =>
            {
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
            }, "SELECT id, dataId, characterName, level FROM characters WHERE partyId=@id",
                new MySqlParameter("@id", id));
            return result;
        }

        public override async Task UpdatePartyLeader(int id, string leaderId)
        {
            await ExecuteNonQuery("UPDATE party SET leaderId=@leaderId WHERE id=@id",
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@id", id));
        }

        public override async Task UpdateParty(int id, bool shareExp, bool shareItem)
        {
            await ExecuteNonQuery("UPDATE party SET shareExp=@shareExp, shareItem=@shareItem WHERE id=@id",
                new MySqlParameter("@shareExp", shareExp),
                new MySqlParameter("@shareItem", shareItem),
                new MySqlParameter("@id", id));
        }

        public override async Task DeleteParty(int id)
        {
            await ExecuteNonQuery("DELETE FROM party WHERE id=@id;" +
                "UPDATE characters SET partyId=0 WHERE partyId=@id;",
                new MySqlParameter("@id", id));
        }

        public override async Task UpdateCharacterParty(string characterId, int partyId)
        {
            await ExecuteNonQuery("UPDATE characters SET partyId=@partyId WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@partyId", partyId));
        }
    }
}
