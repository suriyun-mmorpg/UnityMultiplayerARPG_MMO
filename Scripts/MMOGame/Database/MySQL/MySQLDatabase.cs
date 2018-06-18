using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using UnityEngine;

namespace Insthync.MMOG
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
            var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", GenericUtils.GetMD5(password)));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override async Task<bool> ValidateAccessToken(string userId, string accessToken)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
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
                new MySqlParameter("@password", GenericUtils.GetMD5(password)),
                new MySqlParameter("@email", ""),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override async Task<long> FindUsername(string username)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }
    }
}
