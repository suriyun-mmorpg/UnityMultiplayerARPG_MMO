using Cysharp.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override async UniTask<int> CreateGuild(string guildName, string leaderId)
        {
            int id = 0;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    id = reader.GetInt32(0);
            }, "INSERT INTO guild (guildName, leaderId) VALUES (@guildName, @leaderId);" +
                "SELECT LAST_INSERT_ID();",
                new MySqlParameter("@guildName", guildName),
                new MySqlParameter("@leaderId", leaderId));
            if (id > 0)
                await ExecuteNonQuery("UPDATE characters SET guildId=@id WHERE id=@leaderId",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@leaderId", leaderId));
            return id;
        }

        public override async UniTask<GuildData> ReadGuild(int id, GuildRoleData[] defaultGuildRoles)
        {
            GuildData result = null;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    result = new GuildData(id,
                        reader.GetString(0),
                        reader.GetString(1),
                        defaultGuildRoles);
                    result.level = reader.GetInt16(2);
                    result.exp = reader.GetInt32(3);
                    result.skillPoint = reader.GetInt16(4);
                    result.guildMessage = reader.GetString(5);
                    result.gold = reader.GetInt32(6);
                }
            }, "SELECT guildName, leaderId, level, exp, skillPoint, guildMessage, gold FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            // Read relates data if guild exists
            if (result != null)
            {
                // Guild roles
                await ExecuteReader((reader) =>
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
                await ExecuteReader((reader) =>
                {
                    SocialCharacterData guildMemberData;
                    while (reader.Read())
                    {
                        // Get some required data, other data will be set at server side
                        guildMemberData = new SocialCharacterData();
                        guildMemberData.id = reader.GetString(0);
                        guildMemberData.dataId = reader.GetInt32(1);
                        guildMemberData.characterName = reader.GetString(2);
                        guildMemberData.level = reader.GetInt16(3);
                        result.AddMember(guildMemberData, reader.GetByte(4));
                    }
                }, "SELECT id, dataId, characterName, level, guildRole FROM characters WHERE guildId=@id",
                    new MySqlParameter("@id", id));
                // Guild skills
                await ExecuteReader((reader) =>
                {
                    while (reader.Read())
                    {
                        result.SetSkillLevel(reader.GetInt32(0), reader.GetInt16(1));
                    }
                }, "SELECT dataId, level FROM guildskill WHERE guildId=@id",
                    new MySqlParameter("@id", id));
            }
            return result;
        }

        public override async UniTask UpdateGuildLevel(int id, short level, int exp, short skillPoint)
        {
            await ExecuteNonQuery("UPDATE guild SET level=@level, exp=@exp, skillPoint=@skillPoint WHERE id=@id",
                new MySqlParameter("@level", level),
                new MySqlParameter("@exp", exp),
                new MySqlParameter("@skillPoint", skillPoint),
                new MySqlParameter("@id", id));
        }

        public override async UniTask UpdateGuildLeader(int id, string leaderId)
        {
            await ExecuteNonQuery("UPDATE guild SET leaderId=@leaderId WHERE id=@id",
                new MySqlParameter("@leaderId", leaderId),
                new MySqlParameter("@id", id));
        }

        public override async UniTask UpdateGuildMessage(int id, string guildMessage)
        {
            await ExecuteNonQuery("UPDATE guild SET guildMessage=@guildMessage WHERE id=@id",
                new MySqlParameter("@guildMessage", guildMessage),
                new MySqlParameter("@id", id));
        }

        public override async UniTask UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            await ExecuteNonQuery("DELETE FROM guildrole WHERE guildId=@guildId AND guildRole=@guildRole",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@guildRole", guildRole));
            await ExecuteNonQuery("INSERT INTO guildrole (guildId, guildRole, name, canInvite, canKick, shareExpPercentage) " +
                "VALUES (@guildId, @guildRole, @name, @canInvite, @canKick, @shareExpPercentage)",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@guildRole", guildRole),
                new MySqlParameter("@name", name),
                new MySqlParameter("@canInvite", canInvite),
                new MySqlParameter("@canKick", canKick),
                new MySqlParameter("@shareExpPercentage", shareExpPercentage));
        }

        public override async UniTask UpdateGuildMemberRole(string characterId, byte guildRole)
        {
            await ExecuteNonQuery("UPDATE characters SET guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override async UniTask UpdateGuildSkillLevel(int id, int dataId, short level, short skillPoint)
        {
            await ExecuteNonQuery("DELETE FROM guildskill WHERE guildId=@guildId AND dataId=@dataId",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@dataId", dataId));
            await ExecuteNonQuery("INSERT INTO guildskill (guildId, dataId, level) " +
                "VALUES (@guildId, @dataId, @level)",
                new MySqlParameter("@guildId", id),
                new MySqlParameter("@dataId", dataId),
                new MySqlParameter("@level", level));
            await ExecuteNonQuery("UPDATE guild SET skillPoint=@skillPoint WHERE id=@id",
                new MySqlParameter("@skillPoint", skillPoint),
                new MySqlParameter("@id", id));
        }

        public override async UniTask DeleteGuild(int id)
        {
            await ExecuteNonQuery("DELETE FROM guild WHERE id=@id;" +
                "UPDATE characters SET guildId=0 WHERE guildId=@id;",
                new MySqlParameter("@id", id));
        }

        public override async UniTask<long> FindGuildName(string guildName)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM guild WHERE guildName LIKE @guildName",
                new MySqlParameter("@guildName", guildName));
            return result != null ? (long)result : 0;
        }

        public override async UniTask UpdateCharacterGuild(string characterId, int guildId, byte guildRole)
        {
            await ExecuteNonQuery("UPDATE characters SET guildId=@guildId, guildRole=@guildRole WHERE id=@characterId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@guildId", guildId),
                new MySqlParameter("@guildRole", guildRole));
        }

        public override async UniTask<int> GetGuildGold(int guildId)
        {
            int gold = 0;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    gold = reader.GetInt32(0);
            }, "SELECT gold FROM guild WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", guildId));
            return gold;
        }

        public override async UniTask UpdateGuildGold(int guildId, int gold)
        {
            await ExecuteNonQuery("UPDATE guild SET gold=@gold WHERE id=@id",
                new MySqlParameter("@id", guildId),
                new MySqlParameter("@gold", gold));
        }
    }
}
