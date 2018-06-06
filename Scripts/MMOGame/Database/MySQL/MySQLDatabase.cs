using MySql.Data.MySqlClient;

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
        public string address = "localhost";
        public int port = 3306;
        public string username = "root";
        public string password = "";
        public string dbName = "mmorpgtemplate";
        private MySqlConnection connection;

        public void SetupConnection()
        {
            var connectionString = "Server=" + address + ";" +
                "Port=" + port + ";" +
                "Uid=" + username + ";" +
                (string.IsNullOrEmpty(password) ? "" : "Pwd=\"" + password + "\";") +
                "Database=" + dbName + ";";
            if (connection != null)
                connection.Dispose();
            connection = new MySqlConnection(connectionString);
        }

        public override void Connect()
        {
            SetupConnection();
        }

        public override void Disconnect()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        public void SetupConnection(string address, int port, string username, string password, string dbName)
        {
            this.address = address;
            this.port = port;
            this.username = username;
            this.password = password;
            this.dbName = dbName;
            SetupConnection();
        }

        public long ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            long result = 0;
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                cmd.ExecuteNonQuery();
                result = cmd.LastInsertedId;
            }
            connection.Close();
            return result;
        }

        public void ExecuteNonQuery(string sql, params MySqlParameter[] args)
        {
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }

        public object ExecuteScalar(string sql, params MySqlParameter[] args)
        {
            object result;
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                result = cmd.ExecuteScalar();
            }
            connection.Close();
            return result;
        }

        public MySQLRowsReader ExecuteReader(string sql, params MySqlParameter[] args)
        {
            MySQLRowsReader result = new MySQLRowsReader();
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                result.Init(cmd.ExecuteReader());
            }
            connection.Close();
            return result;
        }

        public override bool ValidateUserLogin(string username, string password, out string id)
        {
            id = string.Empty;
            var reader = ExecuteReader("SELECT id FROM userLogin WHERE username=@username AND password=@password LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));

            if (reader.Read())
            {
                id = reader.GetString("id");
                return true;
            }

            return false;
        }

        public override void CreateUserLogin(string username, string password)
        {
            ExecuteNonQuery("INSERT INTO userLogin (id, username, password) VALUES (@id, @username, @password)",
                new MySqlParameter("@id", System.Guid.NewGuid().ToString()),
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));
        }

        public override long FindUsername(string username)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM userLogin WHERE username LIKE @username", 
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }
    }
}
