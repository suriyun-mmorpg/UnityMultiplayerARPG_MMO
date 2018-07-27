using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using MiniJSON;

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

        public async Task<long> ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            var connection = NewConnection();
            connection.Open();
            var result = await ExecuteInsertData(connection, sql, args);
            connection.Close();
            return result;
        }

        public async Task<long> ExecuteInsertData(MySqlConnection connection, string sql, params MySqlParameter[] args)
        {
            long result = 0;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                await cmd.ExecuteNonQueryAsync();
                result = cmd.LastInsertedId;
            }
            return result;
        }

        public async Task<int> ExecuteNonQuery(string sql, params MySqlParameter[] args)
        {
            var connection = NewConnection();
            connection.Open();
            var result = await ExecuteNonQuery(connection, sql, args);
            connection.Close();
            return result;
        }

        public async Task<int> ExecuteNonQuery(MySqlConnection connection, string sql, params MySqlParameter[] args)
        {
            var numRows = 0;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                numRows = await cmd.ExecuteNonQueryAsync();
            }
            return numRows;
        }

        public async Task<object> ExecuteScalar(string sql, params MySqlParameter[] args)
        {
            var connection = NewConnection();
            connection.Open();
            var result = await ExecuteScalar(connection, sql, args);
            connection.Close();
            return result;
        }

        public async Task<object> ExecuteScalar(MySqlConnection connection, string sql, params MySqlParameter[] args)
        {
            object result;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                result = await cmd.ExecuteScalarAsync();
            }
            return result;
        }

        public async Task<MySQLRowsReader> ExecuteReader(string sql, params MySqlParameter[] args)
        {
            var connection = NewConnection();
            connection.Open();
            var result = await ExecuteReader(connection, sql, args);
            connection.Close();
            return result;
        }

        public async Task<MySQLRowsReader> ExecuteReader(MySqlConnection connection, string sql, params MySqlParameter[] args)
        {
            MySQLRowsReader result = new MySQLRowsReader();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                var dataReader = await cmd.ExecuteReaderAsync();
                result.Init(dataReader);
                dataReader.Close();
            }
            return result;
        }

        public override async Task<string> ValidateUserLogin(string username, string password)
        {
            var id = string.Empty;
            var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", GenericUtils.GetMD5(password)),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override async Task<bool> ValidateAccessToken(string userId, string accessToken)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken LIMIT 1",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override async Task<int> GetCash(string userId)
        {
            var cash = 0;
            var reader = await ExecuteReader("SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            if (reader.Read())
                cash = reader.GetInt32("cash");
            return cash;
        }

        public override async Task UpdateAccessToken(string userId, string accessToken)
        {
            await ExecuteNonQuery("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
        }

        public override async Task CreateUserLogin(string username, string password)
        {
            await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", GenericUtils.GetMD5(password)),
                new MySqlParameter("@email", ""),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override async Task<long> FindUsername(string username)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username LIMIT 1",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override async Task<string> FacebookLogin(string fbId, string accessToken)
        {
            var url = "https://graph.facebook.com/" + fbId + "?access_token=" + accessToken + "&fields=id,name,email";
            var webClient = new WebClient();
            var json = await webClient.DownloadStringTaskAsync(url);
            json = json.Replace(@"\u0040", "@");

            var id = string.Empty;
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                var email = (string)dict["email"];
                var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new MySqlParameter("@username", "fb_" + fbId),
                    new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                    new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                        new MySqlParameter("@username", "fb_" + fbId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                        new MySqlParameter("@email", email),
                        new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                    // Read last entry
                    reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                        new MySqlParameter("@username", "fb_" + fbId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                        new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                    if (reader.Read())
                        id = reader.GetString("id");
                }
            }
            return id;
        }

        public override async Task<string> GooglePlayLogin(string idToken)
        {
            var url = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + idToken;
            var webClient = new WebClient();
            var json = await webClient.DownloadStringTaskAsync(url);

            var id = string.Empty;
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("sub") && dict.ContainsKey("email"))
            {
                var gId = (string)dict["sub"];
                var email = (string)dict["email"];
                var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new MySqlParameter("@username", "g_" + gId),
                    new MySqlParameter("@password", GenericUtils.GetMD5(gId)),
                    new MySqlParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                        new MySqlParameter("@username", "g_" + gId),
                        new MySqlParameter("@password", GenericUtils.GetMD5(gId)),
                        new MySqlParameter("@email", email),
                        new MySqlParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                    // Read last entry
                    reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
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
