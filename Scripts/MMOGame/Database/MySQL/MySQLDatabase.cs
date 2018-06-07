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
        private MySqlConnection connection;

        private void Awake()
        {
            connection = new MySqlConnection(GetConnectionString());
            connection.Open();
        }

        private void OnDestroy()
        {
            if (connection != null)
                connection.Close();
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

        public async Task<long> ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            long result = 0;
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                var task = await cmd.ExecuteNonQueryAsync();
                result = cmd.LastInsertedId;
            }
            return result;
        }

        public async Task<int> ExecuteNonQuery(string sql, params MySqlParameter[] args)
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
            var reader = await ExecuteReader("SELECT id FROM userLogin WHERE username=@username AND password=@password LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override async Task CreateUserLogin(string username, string password)
        {
            await ExecuteNonQuery("INSERT INTO userLogin (id, username, password) VALUES (@id, @username, @password)",
                new MySqlParameter("@id", System.Guid.NewGuid().ToString()),
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));
        }

        public override async Task<long> FindUsername(string username)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userLogin WHERE username LIKE @username",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }
    }
}
