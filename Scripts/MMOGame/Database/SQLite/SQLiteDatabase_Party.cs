#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override int CreateParty(bool shareExp, bool shareItem, string leaderId)
        {
            int id = 0;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                    id = reader.GetInt32(0);
            }, "INSERT INTO party (shareExp, shareItem, leaderId) VALUES (@shareExp, @shareItem, @leaderId);" +
                "SELECT LAST_INSERT_ROWID();",
                new SqliteParameter("@shareExp", shareExp),
                new SqliteParameter("@shareItem", shareItem),
                new SqliteParameter("@leaderId", leaderId));
            if (id > 0)
            {
                ExecuteNonQuery("UPDATE characters SET partyId=@id WHERE id=@leaderId",
                    new SqliteParameter("@id", id),
                    new SqliteParameter("@leaderId", leaderId));
            }
            return id;
        }

        public override PartyData ReadParty(int id)
        {
            PartyData result = null;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    result = new PartyData(id,
                        reader.GetBoolean(0),
                        reader.GetBoolean(1),
                        reader.GetString(2));
                }
            }, "SELECT shareExp, shareItem, leaderId FROM party WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", id));
            if (result != null)
            {
                ExecuteReader((reader) =>
                {
                    SocialCharacterData partyMemberData;
                    while (reader.Read())
                    {
                        // Get some required data, other data will be set at server side
                        partyMemberData = new SocialCharacterData();
                        partyMemberData.id = reader.GetString(0);
                        partyMemberData.dataId = reader.GetInt32(1);
                        partyMemberData.characterName = reader.GetString(2);
                        partyMemberData.level = reader.GetInt32(3);
                        result.AddMember(partyMemberData);
                    }
                }, "SELECT id, dataId, characterName, level FROM characters WHERE partyId=@id",
                    new SqliteParameter("@id", id));
            }
            return result;
        }

        public override void UpdatePartyLeader(int id, string leaderId)
        {
            ExecuteNonQuery("UPDATE party SET leaderId=@leaderId WHERE id=@id",
                new SqliteParameter("@leaderId", leaderId),
                new SqliteParameter("@id", id));
        }

        public override void UpdateParty(int id, bool shareExp, bool shareItem)
        {
            ExecuteNonQuery("UPDATE party SET shareExp=@shareExp, shareItem=@shareItem WHERE id=@id",
                new SqliteParameter("@shareExp", shareExp),
                new SqliteParameter("@shareItem", shareItem),
                new SqliteParameter("@id", id));
        }

        public override void DeleteParty(int id)
        {
            ExecuteNonQuery("DELETE FROM party WHERE id=@id;" +
                "UPDATE characters SET partyId=0 WHERE partyId=@id;",
                new SqliteParameter("@id", id));
        }

        public override void UpdateCharacterParty(string characterId, int partyId)
        {
            ExecuteNonQuery("UPDATE characters SET partyId=@partyId WHERE id=@characterId",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@partyId", partyId));
        }
    }
}
#endif