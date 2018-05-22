using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public class MySQLRowsReader
    {
        private readonly List<List<object>> data = new List<List<object>>();
        private int currentRow = -1;
        public int FieldCount { get; private set; }
        public int VisibleFieldCount { get; private set; }
        public int RowCount { get { return data.Count; } }
        public bool HasRows { get { return RowCount > 0; } }

        public void Init(MySqlDataReader dataReader)
        {
            FieldCount = dataReader.FieldCount;
            VisibleFieldCount = dataReader.VisibleFieldCount;
            while (dataReader.Read())
            {
                var buffer = new object[dataReader.FieldCount];
                dataReader.GetValues(buffer);
                data.Add(new List<object>(buffer));
            }
            ResetReader();
        }

        public bool Read()
        {
            if (currentRow + 1 >= RowCount)
                return false;
            ++currentRow;
            return true;
        }

        public System.DateTime GetDateTime(int index)
        {
            return (System.DateTime)data[currentRow][index];
        }

        public char GetChar(int index)
        {
            return (char)data[currentRow][index];
        }

        public string GetString(int index)
        {
            return (string)data[currentRow][index];
        }

        public bool GetBoolean(int index)
        {
            return (bool)data[currentRow][index];
        }

        public short GetInt16(int index)
        {
            return (short)((long)data[currentRow][index]);
        }

        public int GetInt32(int index)
        {
            return (int)((long)data[currentRow][index]);
        }

        public long GetInt64(int index)
        {
            return (long)data[currentRow][index];
        }

        public decimal GetDecimal(int index)
        {
            return (decimal)((double)data[currentRow][index]);
        }

        public float GetFloat(int index)
        {
            return (float)((double)data[currentRow][index]);
        }

        public double GetDouble(int index)
        {
            return (double)data[currentRow][index];
        }

        public void ResetReader()
        {
            currentRow = -1;
        }
    }

    public class MySQLDatabase : BaseDatabase
    {
        public string address = "localhost";
        public int port = 3306;
        public string username = "root";
        public string password = "";
        public string dbName = "MMORPGTemplate";
        private MySqlConnection connection;

        private void Awake()
        {
            var connectionString = "Server=" + address + ";" +
                "Port=" + port + ";" +
                "Uid=" + username + ";" +
                (string.IsNullOrEmpty(password) ? "" : "Pwd=\"" + password + "\";") +
                "Database=" + dbName + ";";
            connection = new MySqlConnection(connectionString);
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

        public override bool ValidateLogin(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public override UserLoginData Register(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public override long FindUsername(string username)
        {
            throw new System.NotImplementedException();
        }

        public override bool CreateCharacter(string userId, PlayerCharacterData characterData)
        {
            throw new System.NotImplementedException();
        }

        public override PlayerCharacterData ReadCharacter(string characterId)
        {
            throw new System.NotImplementedException();
        }

        public override List<LitePlayerCharacterData> ReadCharacters(string userId)
        {
            throw new System.NotImplementedException();
        }

        public override bool UpdateCharacter(PlayerCharacterData characterData)
        {
            throw new System.NotImplementedException();
        }

        public override bool DeleteCharacter(string characterId)
        {
            throw new System.NotImplementedException();
        }

        public override long FindCharacterName(string characterName)
        {
            throw new System.NotImplementedException();
        }
    }
}
