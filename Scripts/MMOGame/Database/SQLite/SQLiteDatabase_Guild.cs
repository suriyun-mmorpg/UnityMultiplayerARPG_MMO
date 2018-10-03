using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override int CreateGuild(string guildName, string leaderId)
        {
            string leaderName = string.Empty;
            var reader = ExecuteReader("SELECT characterName FROM characters WHERE id=@leaderId LIMIT 1",
                new SqliteParameter("@leaderId", leaderId));
            if (reader.Read())
                leaderName = reader.GetString("characterName");

            int id = 0;
            reader = ExecuteReader("INSERT INTO guild (guildName, leaderId, leaderName, level, exp, skillPoint) VALUES (@guildName, @leaderId, @leaderName, @level, @exp, @skillPoint);" +
                "SELECT LAST_INSERT_ROWID();",
                new SqliteParameter("@guildName", guildName),
                new SqliteParameter("@leaderId", leaderId),
                new SqliteParameter("@leaderName", leaderName),
                new SqliteParameter("@level", 1),
                new SqliteParameter("@exp", 0),
                new SqliteParameter("@skillPoint", 0));
            if (reader.Read())
                id = (int)reader.GetInt64(0);
            if (id > 0)
                ExecuteNonQuery("UPDATE characters SET guildId=@id WHERE id=@leaderId",
                    new SqliteParameter("@id", id),
                    new SqliteParameter("@leaderId", leaderId));
            return id;
        }

        public override GuildData ReadGuild(int id)
        {
            GuildData result = null;
            var reader = ExecuteReader("SELECT * FROM guild WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", id));
            if (reader.Read())
            {
                result = new GuildData(id, reader.GetString("guildName"), reader.GetString("leaderId"), reader.GetString("leaderName"));
                reader = ExecuteReader("SELECT id, dataId, characterName, level, guildRole FROM characters WHERE guildId=@id",
                    new SqliteParameter("@id", id));
                SocialCharacterData guildMemberData;
                while (reader.Read())
                {
                    // Get some required data, other data will be set at server side
                    guildMemberData = new SocialCharacterData();
                    guildMemberData.id = reader.GetString("id");
                    guildMemberData.characterName = reader.GetString("characterName");
                    guildMemberData.dataId = reader.GetInt32("dataId");
                    guildMemberData.level = reader.GetInt32("level");
                    result.AddMember(guildMemberData, reader.GetByte("guildRole"));
                }
            }
            return result;
        }

        public override int IncreaseGuildExp(int id, int amount)
        {
            int exp = 0;
            var reader = ExecuteReader("SELECT exp FROM guild WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", id));
            if (reader.Read())
                exp = reader.GetInt32("exp");
            ExecuteNonQuery("UPDATE guild SET exp=@exp WHERE id=@id",
                new SqliteParameter("@exp", exp),
                new SqliteParameter("@id", id));
            return exp;
        }

        public override void UpdateGuildLeader(int id, string leaderId)
        {
            string leaderName = string.Empty;
            var reader = ExecuteReader("SELECT characterName FROM characters WHERE id=@leaderId LIMIT 1",
                new SqliteParameter("@leaderId", leaderId));
            if (reader.Read())
                leaderName = reader.GetString("characterName");
            ExecuteNonQuery("UPDATE guild SET leaderId=@leaderId, leaderName=@leaderName WHERE id=@id",
                new SqliteParameter("@leaderId", leaderId),
                new SqliteParameter("@leaderName", leaderName),
                new SqliteParameter("@id", id));
        }

        public override void UpdateGuildMessage(int id, string message)
        {
            ExecuteNonQuery("UPDATE guild SET message=@message WHERE id=@id",
                new SqliteParameter("@message", message),
                new SqliteParameter("@id", id));
        }

        public override void UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            ExecuteNonQuery("REPLACE INTO guildrole (guildId, guildRole, name, canInvite, canKick, shareExpPercentage) " +
                "VALUES (@guildId, @guildRole, @name, @canInvite, @canKick, @shareExpPercentage)",
                new SqliteParameter("@guildId", id),
                new SqliteParameter("@guildRole", guildRole),
                new SqliteParameter("@name", name),
                new SqliteParameter("@canInvite", canInvite ? 1 : 0),
                new SqliteParameter("@canKick", canKick ? 1 : 0),
                new SqliteParameter("@shareExpPercentage", shareExpPercentage));
        }

        public override void UpdateGuildMemberRole(string characterId, byte guildRole)
        {
            ExecuteNonQuery("UPDATE characters SET guildRole=@guildRole WHERE id=@characterId",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@guildRole", guildRole));
        }

        public override void DeleteGuild(int id)
        {
            ExecuteNonQuery("DELETE FROM guild WHERE id=@id;" +
                "UPDATE characters SET guildId=0 WHERE guildId=@id;",
                new SqliteParameter("@id", id));
        }

        public override void UpdateCharacterGuild(string characterId, int guildId, byte guildRole)
        {
            ExecuteNonQuery("UPDATE characters SET guildId=@guildId, guildRole=@guildRole WHERE id=@characterId",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@guildId", guildId),
                new SqliteParameter("@guildRole", guildRole));
        }
    }
}
