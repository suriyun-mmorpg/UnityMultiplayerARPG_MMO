#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override int CreateGuild(string guildName, string leaderId)
        {
            int id = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                    id = reader.GetInt32(0);
            }, "INSERT INTO guild (guildName, leaderId, options) VALUES (@guildName, @leaderId, @options);" +
                "SELECT LAST_INSERT_ID();",
                new MySqlParameter("@guildName", guildName),
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@options", "{}"));
            if (id > 0)
            {
                ExecuteNonQuerySync("UPDATE characters SET guildId=@id WHERE id=@leaderId",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@leaderId", leaderId));
            }
            return id;
        }

        public override GuildData ReadGuild(int id, GuildRoleData[] defaultGuildRoles)
        {
            GuildData result = null;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                {
                    result = new GuildData(id,
                        reader.GetString(0),
                        reader.GetString(1),
                        defaultGuildRoles);
                    result.level = reader.GetInt32(2);
                    result.exp = reader.GetInt32(3);
                    result.skillPoint = reader.GetInt32(4);
                    result.guildMessage = reader.GetString(5);
                    result.guildMessage2 = reader.GetString(6);
                    result.gold = reader.GetInt32(7);
                    result.score = reader.GetInt32(8);
                    result.options = reader.GetString(9);
                    result.autoAcceptRequests = reader.GetBoolean(10);
                    result.rank = reader.GetInt32(11);
                }
            }, "SELECT `guildName`, `leaderId`, `level`, `exp`, `skillPoint`, `guildMessage`, `guildMessage2`, `gold`, `score`, `options`, `autoAcceptRequests`, `rank` FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            // Read relates data if guild exists
            if (result != null)
            {
                // Guild roles
                ExecuteReaderSync((reader) =>
                {
                    byte guildRole;
                    GuildRoleData guildRoleData;
                    while (reader.Read())
                    {
                        guildRole = reader.GetByte(0);
                        guildRoleData = new GuildRoleData();
                        guildRoleData.roleName = reader.GetString(1);
                        guildRoleData.canInvite = reader.GetBoolean(2);
                        guildRoleData.canKick = reader.GetBoolean(3);
                        guildRoleData.shareExpPercentage = reader.GetByte(4);
                        result.SetRole(guildRole, guildRoleData);
                    }
                }, "SELECT guildRole, name, canInvite, canKick, shareExpPercentage FROM guildrole WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                // Guild members
                ExecuteReaderSync((reader) =>
                {
                    SocialCharacterData guildMemberData;
                    while (reader.Read())
                    {
                        // Get some required data, other data will be set at server side
                        guildMemberData = new SocialCharacterData();
                        guildMemberData.id = reader.GetString(0);
                        guildMemberData.dataId = reader.GetInt32(1);
                        guildMemberData.characterName = reader.GetString(2);
                        guildMemberData.level = reader.GetInt32(3);
                        result.AddMember(guildMemberData, reader.GetByte(4));
                    }
                }, "SELECT id, dataId, characterName, level, guildRole FROM characters WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                // Guild skills
                ExecuteReaderSync((reader) =>
                {
                    while (reader.Read())
                    {
                        result.SetSkillLevel(reader.GetInt32(0), reader.GetInt32(1));
                    }
                }, "SELECT dataId, level FROM guildskill WHERE guildId=@id",
                    new MySqlParameter("@id", id));
            }
            return result;
        }

        public override void UpdateGuildLevel(int id, int level, int exp, int skillPoint)
        {
            ExecuteNonQuerySync("UPDATE guild SET level=@level, exp=@exp, skillPoint=@skillPoint WHERE id=@id",
                new MySqlParameter("@level", level),
                new MySqlParameter("@exp", exp),
                new MySqlParameter("@skillPoint", skillPoint),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildLeader(int id, string leaderId)
        {
            ExecuteNonQuerySync("UPDATE guild SET leaderId=@leaderId WHERE id=@id",
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildMessage(int id, string guildMessage)
        {
            ExecuteNonQuerySync("UPDATE guild SET guildMessage=@guildMessage WHERE id=@id",
                new MySqlParameter("@guildMessage", guildMessage),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildMessage2(int id, string guildMessage)
        {
            ExecuteNonQuerySync("UPDATE guild SET guildMessage2=@guildMessage WHERE id=@id",
                new MySqlParameter("@guildMessage", guildMessage),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildScore(int id, int score)
        {
            ExecuteNonQuerySync("UPDATE guild SET score=@score WHERE id=@id",
                new MySqlParameter("@score", score),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildOptions(int id, string options)
        {
            ExecuteNonQuerySync("UPDATE guild SET options=@options WHERE id=@id",
                new MySqlParameter("@options", options),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildAutoAcceptRequests(int id, bool autoAcceptRequests)
        {
            ExecuteNonQuerySync("UPDATE guild SET autoAcceptRequests=@autoAcceptRequests WHERE id=@id",
                new MySqlParameter("@autoAcceptRequests", autoAcceptRequests),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildRank(int id, int rank)
        {
            ExecuteNonQuerySync("UPDATE guild SET rank=@rank WHERE id=@id",
                new MySqlParameter("@rank", rank),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            ExecuteNonQuerySync("DELETE FROM guildrole WHERE guildId=@guildId AND guildRole=@guildRole",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@guildRole", guildRole));
            ExecuteNonQuerySync("INSERT INTO guildrole (guildId, guildRole, name, canInvite, canKick, shareExpPercentage) " +
                "VALUES (@guildId, @guildRole, @name, @canInvite, @canKick, @shareExpPercentage)",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@guildRole", guildRole),
                new MySqlParameter("@name", name),
                new MySqlParameter("@canInvite", canInvite),
                new MySqlParameter("@canKick", canKick),
                new MySqlParameter("@shareExpPercentage", shareExpPercentage));
        }

        public override void UpdateGuildMemberRole(string characterId, byte guildRole)
        {
            ExecuteNonQuerySync("UPDATE characters SET guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override void UpdateGuildSkillLevel(int id, int dataId, int level, int skillPoint)
        {
            ExecuteNonQuerySync("DELETE FROM guildskill WHERE guildId=@guildId AND dataId=@dataId",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@dataId", dataId));
            ExecuteNonQuerySync("INSERT INTO guildskill (guildId, dataId, level) " +
                "VALUES (@guildId, @dataId, @level)",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@dataId", dataId),
                new MySqlParameter("@level", level));
            ExecuteNonQuerySync("UPDATE guild SET skillPoint=@skillPoint WHERE id=@id",
                new MySqlParameter("@skillPoint", skillPoint),
                new MySqlParameter("@id", id));
        }

        public override void DeleteGuild(int id)
        {
            ExecuteNonQuerySync("DELETE FROM guild WHERE id=@id;" +
                "UPDATE characters SET guildId=0 WHERE guildId=@id;",
                new MySqlParameter("@id", id));
        }

        public override long FindGuildName(string guildName)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM guild WHERE guildName LIKE @guildName",
                new MySqlParameter("@guildName", guildName));
            return result != null ? (long)result : 0;
        }

        public override void UpdateCharacterGuild(string characterId, int guildId, byte guildRole)
        {
            ExecuteNonQuerySync("UPDATE characters SET guildId=@guildId, guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildId", guildId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override int GetGuildGold(int guildId)
        {
            int gold = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                    gold = reader.GetInt32(0);
            }, "SELECT gold FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", guildId));
            return gold;
        }

        public override void UpdateGuildGold(int guildId, int gold)
        {
            ExecuteNonQuerySync("UPDATE guild SET gold=@gold WHERE id=@id",
                new MySqlParameter("@id", guildId),
                new MySqlParameter("@gold", gold));
        }
    }
}
#endif