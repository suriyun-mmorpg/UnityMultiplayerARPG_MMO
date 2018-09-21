using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override async Task<int> CreateParty(bool shareExp, bool shareItem, string leaderId)
        {
            int id = 0;
            var reader = await ExecuteReader("INSERT INTO party (shareExp, shareItem, leaderId) VALUES (@shareExp, @shareItem, @leaderId);" +
                "SELECT LAST_INSERT_ROWID();",
                new SqliteParameter("@shareExp", shareExp ? 1 : 0),
                new SqliteParameter("@shareItem", shareItem ? 1 : 0),
                new SqliteParameter("@leaderId", leaderId));
            if (reader.Read())
                id = (int)reader.GetInt64(0);
            if (id > 0)
                await ExecuteNonQuery("UPDATE characters SET partyId=@id WHERE id=@leaderId",
                    new SqliteParameter("@id", id),
                    new SqliteParameter("@leaderId", leaderId));
            return id;
        }

        public override async Task<PartyData> ReadParty(int id)
        {
            PartyData result = null;
            var reader = await ExecuteReader("SELECT * FROM party WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", id));
            if (reader.Read())
            {
                result = new PartyData(id, reader.GetBoolean("shareExp"), reader.GetBoolean("shareItem"), reader.GetString("leaderId"));
                reader = await ExecuteReader("SELECT id, dataId, characterName, level FROM characters WHERE partyId=@id",
                    new SqliteParameter("@id", id));
                SocialCharacterData partyMemberData;
                while (reader.Read())
                {
                    // Get some required data, other data will be set at server side
                    partyMemberData = new SocialCharacterData();
                    partyMemberData.id = reader.GetString("id");
                    partyMemberData.characterName = reader.GetString("characterName");
                    partyMemberData.dataId = reader.GetInt32("dataId");
                    partyMemberData.level = reader.GetInt32("level");
                    result.AddMember(partyMemberData);
                }
            }
            return result;
        }

        public override async Task UpdateParty(int id, bool shareExp, bool shareItem)
        {
            await ExecuteNonQuery("UPDATE party SET shareExp=@shareExp, shareItem=@shareItem WHERE id=@id",
                new SqliteParameter("@shareExp", shareExp),
                new SqliteParameter("@shareItem", shareItem),
                new SqliteParameter("@id", id));
        }

        public override async Task DeleteParty(int id)
        {
            await ExecuteNonQuery("DELETE FROM party WHERE id=@id;" +
                "UPDATE characters SET partyId=0 WHERE partyId=@id;",
                new SqliteParameter("@id", id));
        }

        public override async Task SetCharacterParty(string characterId, int partyId)
        {
            await ExecuteNonQuery("UPDATE characters SET partyId=@partyId WHERE id=@characterId",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@partyId", partyId));
        }
    }
}
