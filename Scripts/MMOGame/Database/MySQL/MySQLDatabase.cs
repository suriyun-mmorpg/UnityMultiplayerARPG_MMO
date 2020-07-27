using MySqlConnector;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using MiniJSON;
using System.IO;

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

            // Migrate data
            try
            {
                // Avoid exception which occuring when `dataId` field not found
                foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
                {
                    ExecuteNonQuery("UPDATE buildings SET entityId=@entityId, dataId=0 WHERE dataId=@dataId",
                        new MySqlParameter("entityId", prefab.EntityId),
                        new MySqlParameter("dataId", prefab.name.GenerateHashId()));
                }
            } catch { }
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

        public long ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            long result = ExecuteInsertData(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public long ExecuteInsertData(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
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
                cmd.ExecuteNonQuery();
                result = cmd.LastInsertedId;
            }
            if (createLocalConnection)
                connection.Close();
            return result;
        }

        public int ExecuteNonQuery(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            int result = ExecuteNonQuery(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public int ExecuteNonQuery(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
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
                numRows = cmd.ExecuteNonQuery();
            }
            if (createLocalConnection)
                connection.Close();
            return numRows;
        }

        public object ExecuteScalar(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            object result = ExecuteScalar(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public object ExecuteScalar(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
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
                result = cmd.ExecuteScalar();
            }
            if (createLocalConnection)
                connection.Close();
            return result;
        }

        public MySQLRowsReader ExecuteReader(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySQLRowsReader result = ExecuteReader(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public MySQLRowsReader ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
                createLocalConnection = true;
            }
            MySQLRowsReader result = new MySQLRowsReader();
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                MySqlDataReader dataReader = cmd.ExecuteReader();
                result.Init(dataReader);
                dataReader.Close();
            }
            if (createLocalConnection)
                connection.Close();
            return result;
        }

        public override string ValidateUserLogin(string username, string password)
        {
            string id = string.Empty;
            MySQLRowsReader reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", GenericUtils.GetMD5(password)),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override bool ValidateAccessToken(string userId, string accessToken)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override byte GetUserLevel(string userId)
        {
            byte userLevel = 0;
            MySQLRowsReader reader = ExecuteReader("SELECT userLevel FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            if (reader.Read())
                userLevel = (byte)reader.GetSByte("userLevel");
            return userLevel;
        }

        public override int GetGold(string userId)
        {
            int gold = 0;
            MySQLRowsReader reader = ExecuteReader("SELECT gold FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            if (reader.Read())
                gold = reader.GetInt32("gold");
            return gold;
        }

        public override int IncreaseGold(string userId, int amount)
        {
            int gold = GetGold(userId);
            gold += amount;
            ExecuteNonQuery("UPDATE userlogin SET gold=@gold WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@gold", gold));
            return gold;
        }

        public override int DecreaseGold(string userId, int amount)
        {
            int gold = GetGold(userId);
            if (gold - amount >= 0)
            {
                gold -= amount;
                ExecuteNonQuery("UPDATE userlogin SET gold=@gold WHERE id=@id",
                    new MySqlParameter("@id", userId),
                    new MySqlParameter("@gold", gold));
            }
            return gold;
        }

        public override int GetCash(string userId)
        {
            int cash = 0;
            MySQLRowsReader reader = ExecuteReader("SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            if (reader.Read())
                cash = reader.GetInt32("cash");
            return cash;
        }

        public override int IncreaseCash(string userId, int amount)
        {
            int cash = GetCash(userId);
            cash += amount;
            ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@cash", cash));
            return cash;
        }

        public override int DecreaseCash(string userId, int amount)
        {
            int cash = GetCash(userId);
            if (cash - amount >= 0)
            {
                cash -= amount;
                ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                    new MySqlParameter("@id", userId),
                    new MySqlParameter("@cash", cash));
            }
            return cash;
        }

        public override void UpdateAccessToken(string userId, string accessToken)
        {
            ExecuteNonQuery("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
        }

        public override void CreateUserLogin(string username, string password)
        {
            ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", GenericUtils.GetMD5(password)),
                new MySqlParameter("@email", ""),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override long FindUsername(string username)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }
    }
}
