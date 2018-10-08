using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override int CreateGuild(string guildName, string leaderId)
        {
            int id = 0;
            var reader = ExecuteReader("INSERT INTO guild (guildName, leaderId, level, exp, skillPoint) VALUES (@guildName, @leaderId, @level, @exp, @skillPoint);" +
                "SELECT LAST_INSERT_ID();",
                new MySqlParameter("@guildName", guildName),
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@level", 1),
                new MySqlParameter("@exp", (object)0),
                new MySqlParameter("@skillPoint", (object)0));
            if (reader.Read())
                id = (int)reader.GetUInt64(0);
            if (id > 0)
                ExecuteNonQuery("UPDATE characters SET guildId=@id WHERE id=@leaderId",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@leaderId", leaderId));
            return id;
        }

        public override GuildData ReadGuild(int id)
        {
            GuildData result = null;
            var reader = ExecuteReader("SELECT * FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            if (reader.Read())
            {
                result = new GuildData(id, reader.GetString("guildName"), reader.GetString("leaderId"));
                result.level = reader.GetInt32("level");
                result.exp = reader.GetInt32("exp");
                result.skillPoint = reader.GetInt32("skillPoint");
                result.guildMessage = reader.GetString("guildMessage");

                reader = ExecuteReader("SELECT * FROM guildrole WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                byte guildRole;
                GuildRole guildRoleData;
                while (reader.Read())
                {
                    guildRole = (byte)reader.GetInt32("guildRole");
                    guildRoleData = new GuildRole();
                    guildRoleData.roleName = reader.GetString("name");
                    guildRoleData.canInvite = reader.GetBoolean("canInvite");
                    guildRoleData.canKick = reader.GetBoolean("canKick");
                    guildRoleData.shareExpPercentage = (byte)reader.GetInt32("shareExpPercentage");
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
                    guildMemberData.level = reader.GetInt32("level");
                    result.AddMember(guildMemberData, (byte)reader.GetInt32("guildRole"));
                }
            }
            return result;
        }

        public override int IncreaseGuildExp(int id, int amount)
        {
            int exp = 0;
            var reader = ExecuteReader("SELECT exp FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            if (reader.Read())
                exp = reader.GetInt32("exp");
            ExecuteNonQuery("UPDATE guild SET exp=@exp WHERE id=@id",
                new MySqlParameter("@exp", exp),
                new MySqlParameter("@id", id));
            return exp;
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
            ExecuteNonQuery("REPLACE INTO guildrole (guildId, guildRole, name, canInvite, canKick, shareExpPercentage) " +
                "VALUES (@guildId, @guildRole, @name, @canInvite, @canKick, @shareExpPercentage)",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@guildRole", guildRole),
                new MySqlParameter("@name", name),
                new MySqlParameter("@canInvite", canInvite ? 1 : 0),
                new MySqlParameter("@canKick", canKick ? 1 : 0),
                new MySqlParameter("@shareExpPercentage", shareExpPercentage));
        }

        public override void UpdateGuildMemberRole(string characterId, byte guildRole)
        {
            ExecuteNonQuery("UPDATE characters SET guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override void DeleteGuild(int id)
        {
            ExecuteNonQuery("DELETE FROM guild WHERE id=@id;" +
                "UPDATE characters SET guildId=0 WHERE guildId=@id;",
                new MySqlParameter("@id", id));
        }

        public override void UpdateCharacterGuild(string characterId, int guildId, byte guildRole)
        {
            ExecuteNonQuery("UPDATE characters SET guildId=@guildId, guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildId", guildId),
                new MySqlParameter("@guildRole", guildRole));
        }
    }
}
