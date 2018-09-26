using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using MiniJSON;
using System.IO;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase : BaseDatabase
    {
        public enum InventoryType : byte
        {
            NonEquipItems,
            EquipItems,
            EquipWeaponRight,
            EquipWeaponLeft,
        }
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
            var configFilePath = "./config/mySqlConfig.json";
            var jsonConfig = new Dictionary<string, object>();
            Debug.Log("[MySQLDatabase] Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                Debug.Log("[MySQLDatabase] Found config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                jsonConfig = Json.Deserialize(dataAsJson) as Dictionary<string, object>;
            }

            ConfigReader.ReadConfigs(jsonConfig, "mySqlAddress", out address, address);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlPort", out port, port);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlUsername", out username, username);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlPassword", out password, password);
            ConfigReader.ReadConfigs(jsonConfig, "mySqlDbName", out dbName, dbName);
        }

        public string GetConnectionString()
        {
            var connectionString = "Server=" + address + ";" +
                "Port=" + port + ";" +
                "Uid=" + username + ";" +
                (string.IsNullOrEmpty(password) ? "" : "Pwd=\"" + password + "\";") +
                "Database=" + dbName + ";";
            return connectionString;
        }

        public MySqlConnection NewConnection()
        {
            return new MySqlConnection(GetConnectionString());
        }

        public long ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            var connection = NewConnection();
            connection.Open();
            var result = ExecuteInsertData(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public long ExecuteInsertData(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            var createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
                createLocalConnection = true;
            }
            long result = 0;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (var arg in args)
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
            var connection = NewConnection();
            connection.Open();
            var result = ExecuteNonQuery(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public int ExecuteNonQuery(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            var createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
                createLocalConnection = true;
            }
            var numRows = 0;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (var arg in args)
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
            var connection = NewConnection();
            connection.Open();
            var result = ExecuteScalar(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public object ExecuteScalar(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            var createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
                createLocalConnection = true;
            }
            object result;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (var arg in args)
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
            var connection = NewConnection();
            connection.Open();
            var result = ExecuteReader(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public MySQLRowsReader ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            var createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                connection.Open();
                createLocalConnection = true;
            }
            var result = new MySQLRowsReader();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                var dataReader = cmd.ExecuteReader();
                result.Init(dataReader);
                dataReader.Close();
            }
            if (createLocalConnection)
                connection.Close();
            return result;
        }

        public override string ValidateUserLogin(string username, string password)
        {
            var id = string.Empty;
            var reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", GenericUtils.GetMD5(password)),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override bool ValidateAccessToken(string userId, string accessToken)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override int GetCash(string userId)
        {
            var cash = 0;
            var reader = ExecuteReader("SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            if (reader.Read())
                cash = reader.GetInt32("cash");
            return cash;
        }

        public override int IncreaseCash(string userId, int amount)
        {
            var cash = GetCash(userId);
            cash += amount;
            ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@cash", cash));
            return cash;
        }

        public override int DecreaseCash(string userId, int amount)
        {
            var cash = GetCash(userId);
            cash -= amount;
            ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@cash", cash));
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
            var result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override string FacebookLogin(string fbId, string accessToken)
        {
            var url = "https://graph.facebook.com/" + fbId + "?access_token=" + accessToken + "&fields=id,name,email";
            var webClient = new WebClient();
            var json = webClient.DownloadString(url);
            json = json.Replace(@"\u0040", "@");

            var id = string.Empty;
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                var email = (string)dict["email"];
                var reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new MySqlParameter("@username", "fb_" + fbId),
                    new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                    new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                        new MySqlParameter("@username", "fb_" + fbId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                        new MySqlParameter("@email", email),
                        new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                    // Read last entry
                    reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                        new MySqlParameter("@username", "fb_" + fbId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                        new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                    if (reader.Read())
                        id = reader.GetString("id");
                }
            }
            return id;
        }

        public override string GooglePlayLogin(string idToken)
        {
            var url = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + idToken;
            var webClient = new WebClient();
            var json = webClient.DownloadString(url);

            var id = string.Empty;
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("sub") && dict.ContainsKey("email"))
            {
                var gId = (string)dict["sub"];
                var email = (string)dict["email"];
                var reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new MySqlParameter("@username", "g_" + gId),
                    new MySqlParameter("@password", GenericUtils.GetMD5(gId)),
                    new MySqlParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                        new MySqlParameter("@username", "g_" + gId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(gId)),
                        new MySqlParameter("@email", email),
                        new MySqlParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                    // Read last entry
                    reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                        new MySqlParameter("@username", "g_" + gId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(gId)),
                        new MySqlParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                    if (reader.Read())
                        id = reader.GetString("id");
                }
            }
            return id;
        }
    }
}
