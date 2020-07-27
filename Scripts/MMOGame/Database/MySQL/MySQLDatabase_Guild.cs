using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override int CreateGuild(string guildName, string leaderId)
        {
            int id = 0;
            MySQLRowsReader reader = ExecuteReader("INSERT INTO guild (guildName, leaderId) VALUES (@guildName, @leaderId);" +
                "SELECT LAST_INSERT_ID();",
                new MySqlParameter("@guildName", guildName),
                new MySqlParameter("@leaderId", leaderId));
            if (reader.Read())
                id = (int)reader.GetUInt64(0);
            if (id > 0)
                ExecuteNonQuery("UPDATE characters SET guildId=@id WHERE id=@leaderId",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@leaderId", leaderId));
            return id;
        }

        public override GuildData ReadGuild(int id, GuildRoleData[] defaultGuildRoles)
        {
            GuildData result = null;
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            if (reader.Read())
            {
                result = new GuildData(id, reader.GetString("guildName"), reader.GetString("leaderId"), defaultGuildRoles);
                result.level = reader.GetInt16("level");
                result.exp = reader.GetInt32("exp");
                result.skillPoint = reader.GetInt16("skillPoint");
                result.guildMessage = reader.GetString("guildMessage");
                result.gold = reader.GetInt32("gold");

                reader = ExecuteReader("SELECT * FROM guildrole WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                byte guildRole;
                GuildRoleData guildRoleData;
                while (reader.Read())
                {
                    guildRole = reader.GetByte("guildRole");
                    guildRoleData = new GuildRoleData();
                    guildRoleData.roleName = reader.GetString("name");
                    guildRoleData.canInvite = reader.GetBoolean("canInvite");
                    guildRoleData.canKick = reader.GetBoolean("canKick");
                    guildRoleData.shareExpPercentage = reader.GetByte("shareExpPercentage");
                    result.SetRole(guildRole, guildRoleData);
                }

                reader = ExecuteReader("SELECT id, dataId, characterName, level, guildRole FROM characters WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                SocialCharacterData guildMemberData;
                while (reader.Read())
                {
                    // Get some required data, other data will be set at server side
                    guildMemberData = new SocialCharacterData();
                    guildMemberData.id = reader.GetString("id");
                    guildMemberData.characterName = reader.GetString("characterName");
                    guildMemberData.dataId = reader.GetInt32("dataId");
                    guildMemberData.level = reader.GetInt16("level");
                    result.AddMember(guildMemberData, reader.GetByte("guildRole"));
                }

                reader = ExecuteReader("SELECT dataId, level FROM guildskill WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                while (reader.Read())
                {
                    result.SetSkillLevel(reader.GetInt32("dataId"), reader.GetInt16("level"));
                }
            }
            return result;
        }

        public override void UpdateGuildLevel(int id, short level, int exp, short skillPoint)
        {
            ExecuteNonQuery("UPDATE guild SET level=@level, exp=@exp, skillPoint=@skillPoint WHERE id=@id",
                new MySqlParameter("@level", level),
                new MySqlParameter("@exp", exp),
                new MySqlParameter("@skillPoint", skillPoint),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildLeader(int id, string leaderId)
        {
            ExecuteNonQuery("UPDATE guild SET leaderId=@leaderId WHERE id=@id",
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildMessage(int id, string guildMessage)
        {
            ExecuteNonQuery("UPDATE guild SET guildMessage=@guildMessage WHERE id=@id",
                new MySqlParameter("@guildMessage", guildMessage),
                new MySqlParameter("@id", id));
        }

        public override void UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            ExecuteNonQuery("DELETE FROM guildrole WHERE guildId=@guildId AND guildRole=@guildRole",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@guildRole", guildRole));
            ExecuteNonQuery("INSERT INTO guildrole (guildId, guildRole, name, canInvite, canKick, shareExpPercentage) " +
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
            ExecuteNonQuery("UPDATE characters SET guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override void UpdateGuildSkillLevel(int id, int dataId, short level, short skillPoint)
        {
            ExecuteNonQuery("DELETE FROM guildskill WHERE guildId=@guildId AND dataId=@dataId",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@dataId", dataId));
            ExecuteNonQuery("INSERT INTO guildskill (guildId, dataId, level) " +
                "VALUES (@guildId, @dataId, @level)",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@dataId", dataId),
                new MySqlParameter("@level", level));
            ExecuteNonQuery("UPDATE guild SET skillPoint=@skillPoint WHERE id=@id",
                new MySqlParameter("@skillPoint", skillPoint),
                new MySqlParameter("@id", id));
        }

        public override void DeleteGuild(int id)
        {
            ExecuteNonQuery("DELETE FROM guild WHERE id=@id;" +
                "UPDATE characters SET guildId=0 WHERE guildId=@id;",
                new MySqlParameter("@id", id));
        }

        public override long FindGuildName(string guildName)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM guild WHERE guildName LIKE @guildName",
                new MySqlParameter("@guildName", guildName));
            return result != null ? (long)result : 0;
        }

        public override void UpdateCharacterGuild(string characterId, int guildId, byte guildRole)
        {
            ExecuteNonQuery("UPDATE characters SET guildId=@guildId, guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildId", guildId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override int GetGuildGold(int guildId)
        {
            int gold = 0;
            MySQLRowsReader reader = ExecuteReader("SELECT gold FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", guildId));
            if (reader.Read())
                gold = reader.GetInt32("gold");
            return gold;
        }

        public override void UpdateGuildGold(int guildId, int gold)
        {
            ExecuteNonQuery("UPDATE guild SET gold=@gold WHERE id=@id",
                new MySqlParameter("@id", guildId),
                new MySqlParameter("@gold", gold));
        }
    }
}
