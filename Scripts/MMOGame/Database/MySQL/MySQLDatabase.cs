using MySqlConnector;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using MiniJSON;
using System.IO;
using System.Threading.Tasks;
using System;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase : BaseDatabase
    {
        [SerializeField]
        private string address = "127.0.0.1";
        [SerializeField]
        private int port = 3306;
        [SerializeField]
        private string username = "root";
        [SerializeField]
        private string password = "";
        [SerializeField]
        private string dbName = "mmorpgtemplate";

        public override void Initialize()
        {
            // Json file read
            string configFilePath = "./config/mySqlConfig.json";
            Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
            Logging.Log(ToString(), "Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                Logging.Log(ToString(), "Found config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                jsonConfig = Json.Deserialize(dataAsJson) as Dictionary<string, object>;
            }

            ConfigReader.ReadConfigs(jsonConfig, "mySqlAddress", out address, address);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlPort", out port, port);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlUsername", out username, username);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlPassword", out password, password);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlDbName", out dbName, dbName);

            Migration();
        }

        private async void Migration()
        {
            // 1.57b
            string migrationId = "1.57b";
            if (!await HasMigrationId(migrationId))
            {
                // Migrate data
                try
                {
                    // Avoid exception which occuring when `dataId` field not found
                    foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
                    {
                        await ExecuteNonQuery("UPDATE buildings SET entityId=@entityId, dataId=0 WHERE dataId=@dataId",
                            new MySqlParameter("entityId", prefab.EntityId),
                            new MySqlParameter("dataId", prefab.name.GenerateHashId()));
                    }
                }
                catch { }
                // Migrate fields
                try
                {
                    // Avoid exception which occuring when `dataId` field not found
                    await ExecuteNonQuery("ALTER TABLE buildings DROP dataId;");
                }
                catch { }
                // Insert migrate history
                InsertMigrationId(migrationId);
            }
        }

        private async Task<bool> HasMigrationId(string migrationId)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM __migrations WHERE migrationId=@migrationId", new MySqlParameter("@migrationId", migrationId));
            long count = result != null ? (long)result : 0;
            return count > 0;
        }

        public async void InsertMigrationId(string migrationId)
        {
            await ExecuteNonQuery("INSERT INTO __migrations (migrationId) VALUES (@migrationId)", new MySqlParameter("@migrationId", migrationId));
        }

        public string GetConnectionString()
        {
            string connectionString = "Server=" + address + ";" +
                "Port=" + port + ";" +
                "Uid=" + username + ";" +
                (string.IsNullOrEmpty(password) ? "" : "Pwd=\"" + password + "\";") +
                "Database=" + dbName + ";" +
                "SSL Mode=None;";
            return connectionString;
        }

        public MySqlConnection NewConnection()
        {
            return new MySqlConnection(GetConnectionString());
        }

        private async Task OpenConnection(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
            }
            catch (MySqlException ex)
            {
                Logging.LogException(ex);
            }
        }

        public async Task<long> ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            long result = await ExecuteInsertData(connection, null, sql, args);
            await connection.CloseAsync();
            return result;
        }

        public async Task<long> ExecuteInsertData(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            long result = 0;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                await cmd.ExecuteNonQueryAsync();
                result = cmd.LastInsertedId;
            }
            if (createLocalConnection)
                await connection.CloseAsync();
            return result;
        }

        public async Task<int> ExecuteNonQuery(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            int result = await ExecuteNonQuery(connection, null, sql, args);
            await connection.CloseAsync();
            return result;
        }

        public async Task<int> ExecuteNonQuery(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            int numRows = 0;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                numRows = await cmd.ExecuteNonQueryAsync();
            }
            if (createLocalConnection)
                await connection.CloseAsync();
            return numRows;
        }

        public async Task<object> ExecuteScalar(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            object result = await ExecuteScalar(connection, null, sql, args);
            await connection.CloseAsync();
            return result;
        }

        public async Task<object> ExecuteScalar(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            object result;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                result = await cmd.ExecuteScalarAsync();
            }
            if (createLocalConnection)
                await connection.CloseAsync();
            return result;
        }

        public async Task ExecuteReader(Action<MySqlDataReader> onRead, string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            await ExecuteReader(connection, null, onRead, sql, args);
            await connection.CloseAsync();
        }

        public async Task ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, Action<MySqlDataReader> onRead, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                MySqlDataReader dataReader = await cmd.ExecuteReaderAsync();
                if (onRead != null) onRead.Invoke(dataReader);
                dataReader.Close();
            }
            if (createLocalConnection)
                await connection.CloseAsync();
        }

        public override async Task<string> ValidateUserLogin(string username, string password)
        {
            string id = string.Empty;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    id = reader.GetString(0);
                }
            }, "SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password.GetMD5()),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));

            return id;
        }

        public override async Task<bool> ValidateAccessToken(string userId, string accessToken)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override async Task<byte> GetUserLevel(string userId)
        {
            byte userLevel = 0;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    userLevel = reader.GetByte(0);
            }, "SELECT userLevel FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return userLevel;
        }

        public override async Task<int> GetGold(string userId)
        {
            int gold = 0;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    gold = reader.GetInt32(0);
            }, "SELECT gold FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return gold;
        }

        public override async Task UpdateGold(string userId, int gold)
        {
            await ExecuteNonQuery("UPDATE userlogin SET gold=@gold WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@gold", gold));
        }

        public override async Task<int> GetCash(string userId)
        {
            int cash = 0;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    cash = reader.GetInt32(0);
            }, "SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return cash;
        }

        public override async Task UpdateCash(string userId, int cash)
        {
            await ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@cash", cash));
        }

        public override async Task UpdateAccessToken(string userId, string accessToken)
        {
            await ExecuteNonQuery("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
        }

        public override async Task CreateUserLogin(string username, string password)
        {
            await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password.GetMD5()),
                new MySqlParameter("@email", ""),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override async Task<long> FindUsername(string username)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }
    }
}
