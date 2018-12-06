using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override int CreateGuild(string guildName, string leaderId)
        {
            int id = 0;
            var reader = ExecuteReader("INSERT INTO guild (guildName, leaderId) VALUES (@guildName, @leaderId);" +
                "SELECT LAST_INSERT_ROWID();",
                new SqliteParameter("@guildName", guildName),
                new SqliteParameter("@leaderId", leaderId));
            if (reader.Read())
                id = (int)reader.GetInt64(0);
            if (id > 0)
                ExecuteNonQuery("UPDATE characters SET guildId=@id WHERE id=@leaderId",
                    new SqliteParameter("@id", id),
                    new SqliteParameter("@leaderId", leaderId));
            return id;
        }

        public override GuildData ReadGuild(int id, GuildRoleData[] defaultGuildRoles)
        {
            GuildData result = null;
            var reader = ExecuteReader("SELECT * FROM guild WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", id));
            if (reader.Read())
            {
                result = new GuildData(id, reader.GetString("guildName"), reader.GetString("leaderId"), defaultGuildRoles);
                result.level = (short)reader.GetInt32("level");
                result.exp = reader.GetInt32("exp");
                result.skillPoint = (short)reader.GetInt32("skillPoint");
                result.guildMessage = reader.GetString("guildMessage");

                reader = ExecuteReader("SELECT * FROM guildrole WHERE guildId=@id",
                    new SqliteParameter("@id", id));
                byte guildRole;
                GuildRoleData guildRoleData;
                while (reader.Read())
                {
                    guildRole = (byte)reader.GetInt32("guildRole");
                    guildRoleData = new GuildRoleData();
                    guildRoleData.roleName = reader.GetString("name");
                    guildRoleData.canInvite = reader.GetBoolean("canInvite");
                    guildRoleData.canKick = reader.GetBoolean("canKick");
                    guildRoleData.shareExpPercentage = (byte)reader.GetInt32("shareExpPercentage");
                    result.SetRole(guildRole, guildRoleData);
                }

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
                    guildMemberData.level = reader.GetInt16("level");
                    result.AddMember(guildMemberData, (byte)reader.GetInt32("guildRole"));
                }

                reader = ExecuteReader("SELECT dataId, level FROM guildskill WHERE guildId=@id",
                    new SqliteParameter("@id", id));
                while (reader.Read())
                {
                    result.SetSkillLevel(reader.GetInt32("dataId"), reader.GetInt16("level"));
                }
            }
            return result;
        }

        public override bool IncreaseGuildExp(int id, int increaseExp, int[] expTree, out short resultLevel, out int resultExp, out short resultSkillPoint)
        {
            resultLevel = 1;
            resultExp = 0;
            resultSkillPoint = 0;

            var reader = ExecuteReader("UPDATE guild SET exp=exp+@increaseExp WHERE id=@id;" +
                "SELECT level, exp, skillPoint FROM guild WHERE id=@id LIMIT 1;",
                new SqliteParameter("@increaseExp", increaseExp),
                new SqliteParameter("@id", id));
            if (reader.Read())
            {
                resultLevel = (short)reader.GetInt32("level");
                resultExp = reader.GetInt32("exp");
                resultSkillPoint = (short)reader.GetInt32("skillPoint");
                // Update when guild level is increase
                if (SocialSystemSetting.CalculateIncreasedGuildExp(expTree, resultLevel, resultExp, resultSkillPoint, out resultLevel, out resultExp, out resultSkillPoint))
                {
                    ExecuteNonQuery("UPDATE guild SET level=@level, exp=@exp, skillPoint=@skillPoint WHERE id=@id",
                        new SqliteParameter("@level", resultLevel),
                        new SqliteParameter("@exp", resultExp),
                        new SqliteParameter("@skillPoint", resultSkillPoint),
                        new SqliteParameter("@id", id));
                }
                // Return true if success
                return true;
            }
            return false;
        }

        public override void UpdateGuildLeader(int id, string leaderId)
        {
            ExecuteNonQuery("UPDATE guild SET leaderId=@leaderId WHERE id=@id",
                new SqliteParameter("@leaderId", leaderId),
                new SqliteParameter("@id", id));
        }

        public override void UpdateGuildMessage(int id, string guildMessage)
        {
            ExecuteNonQuery("UPDATE guild SET guildMessage=@guildMessage WHERE id=@id",
                new SqliteParameter("@guildMessage", guildMessage),
                new SqliteParameter("@id", id));
        }

        public override void UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            ExecuteNonQuery("DELETE FROM guildrole WHERE guildId=@guildId AND guildRole=@guildRole",
                new SqliteParameter("@guildId", id),
                new SqliteParameter("@guildRole", guildRole));
            ExecuteNonQuery("INSERT INTO guildrole (guildId, guildRole, name, canInvite, canKick, shareExpPercentage) " +
                "VALUES (@guildId, @guildRole, @name, @canInvite, @canKick, @shareExpPercentage)",
                new SqliteParameter("@guildId", id),
                new SqliteParameter("@guildRole", guildRole),
                new SqliteParameter("@name", name),
                new SqliteParameter("@canInvite", canInvite),
                new SqliteParameter("@canKick", canKick),
                new SqliteParameter("@shareExpPercentage", shareExpPercentage));
        }

        public override void UpdateGuildMemberRole(string characterId, byte guildRole)
        {
            ExecuteNonQuery("UPDATE characters SET guildRole=@guildRole WHERE id=@characterId",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@guildRole", guildRole));
        }

        public override void UpdateGuildSkillLevel(int id, int dataId, short level, short skillPoint)
        {
            ExecuteNonQuery("DELETE FROM guildskill WHERE guildId=@guildId AND dataId=@dataId",
                new SqliteParameter("@guildId", id),
                new SqliteParameter("@dataId", dataId));
            ExecuteNonQuery("INSERT INTO guildskill (guildId, dataId, level) " +
                "VALUES (@guildId, @dataId, @level)",
                new SqliteParameter("@guildId", id),
                new SqliteParameter("@dataId", dataId),
                new SqliteParameter("@level", level));
            ExecuteNonQuery("UPDATE guild SET skillPoint=@skillPoint WHERE id=@id",
                new SqliteParameter("@skillPoint", skillPoint),
                new SqliteParameter("@id", id));
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
