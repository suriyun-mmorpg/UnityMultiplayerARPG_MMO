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

        private void SetupConnection()
        {
            var connectionString = "Server=" + address + ";" +
                "Port=" + port + ";" +
                "Uid=" + username + ";" +
                (string.IsNullOrEmpty(password) ? "" : "Pwd=\"" + password + "\";") +
                "Database=" + dbName + ";";
            connection = new MySqlConnection(connectionString);
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

        public override bool ValidateUserLogin(string username, string password)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM userLogin WHERE username=@username AND password=@password",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));
            return result != null && (long)result > 0;
        }

        public override void CreateUserLogin(string username, string password)
        {
            ExecuteNonQuery("INSERT INTO userLogin (username, password) VALUES (@username, @password)",
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password));
        }

        public override long FindUsername(string username)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM userLogin WHERE username=@username", 
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override void CreateCharacter(string userId, PlayerCharacterData characterData)
        {
            ExecuteInsertData("INSERT INTO character " +
                "(userId, databaseId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ) VALUES " +
                "(@userId, @databaseId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ)",
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@databaseId", characterData.DatabaseId),
                new MySqlParameter("@characterName", characterData.CharacterName),
                new MySqlParameter("@level", characterData.Level),
                new MySqlParameter("@exp", characterData.Exp),
                new MySqlParameter("@currentHp", characterData.CurrentHp),
                new MySqlParameter("@currentMp", characterData.CurrentMp),
                new MySqlParameter("@currentStamina", characterData.CurrentStamina),
                new MySqlParameter("@currentFood", characterData.CurrentFood),
                new MySqlParameter("@currentWater", characterData.CurrentWater),
                new MySqlParameter("@statPoint", characterData.StatPoint),
                new MySqlParameter("@skillPoint", characterData.SkillPoint),
                new MySqlParameter("@gold", characterData.Gold),
                new MySqlParameter("@currentMapName", characterData.CurrentMapName),
                new MySqlParameter("@currentPositionX", characterData.CurrentPosition.x),
                new MySqlParameter("@currentPositionY", characterData.CurrentPosition.y),
                new MySqlParameter("@currentPositionZ", characterData.CurrentPosition.z),
                new MySqlParameter("@respawnMapName", characterData.RespawnMapName),
                new MySqlParameter("@respawnPositionX", characterData.RespawnPosition.x),
                new MySqlParameter("@respawnPositionY", characterData.RespawnPosition.y),
                new MySqlParameter("@respawnPositionZ", characterData.RespawnPosition.z));
        }

        public override PlayerCharacterData ReadCharacter(string characterId,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withHotkeys = true,
            bool withQuests = true)
        {
            var readerCharacter = ExecuteReader("SELECT * FROM character WHERE characterId=@characterId LIMIT 1", new MySqlParameter("@characterId", characterId));
            if (readerCharacter.Read())
            {
                var result = new PlayerCharacterData();
                result.Id = readerCharacter.GetInt64(0).ToString();
                result.DatabaseId = readerCharacter.GetString(1);
                result.CharacterName = readerCharacter.GetString(2);
                result.Level = readerCharacter.GetInt32(3);
                result.Exp = readerCharacter.GetInt32(4);
                result.CurrentHp = readerCharacter.GetInt32(5);
                result.CurrentMp = readerCharacter.GetInt32(6);
                result.CurrentStamina = readerCharacter.GetInt32(7);
                result.CurrentFood = readerCharacter.GetInt32(8);
                result.CurrentWater = readerCharacter.GetInt32(9);
                result.StatPoint = readerCharacter.GetInt32(10);
                result.SkillPoint = readerCharacter.GetInt32(11);
                result.Gold = readerCharacter.GetInt32(12);
                result.CurrentMapName = readerCharacter.GetString(13);
                result.CurrentPosition = new Vector3(readerCharacter.GetFloat(14), readerCharacter.GetFloat(15), readerCharacter.GetFloat(16));
                result.RespawnMapName = readerCharacter.GetString(17);
                result.RespawnPosition = new Vector3(readerCharacter.GetFloat(18), readerCharacter.GetFloat(19), readerCharacter.GetFloat(20));
                result.LastUpdate = readerCharacter.GetInt32(21);
                return result;
            }
            return null;
        }

        public override List<PlayerCharacterData> ReadCharacters(string userId)
        {
            var result = new List<PlayerCharacterData>();
            var readerCharacter = ExecuteReader("SELECT characterId FROM character WHERE userId=@userId ORDER BY lastUpdate DESC", new MySqlParameter("@userId", userId));
            if (readerCharacter.Read())
            {
                var characterId = readerCharacter.GetInt64(0).ToString();
                result.Add(ReadCharacter(characterId, true, true, true, false, true, false, false, false));
            }
            return result;
        }

        public override void UpdateCharacter(PlayerCharacterData characterData)
        {
            throw new System.NotImplementedException();
        }

        public override void DeleteCharacter(string characterId)
        {
            ExecuteNonQuery("DELETE FROM character WHERE id=@characterId", new MySqlParameter("@characterId", characterId));
            ExecuteNonQuery("DELETE FROM characterInventory WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            ExecuteNonQuery("DELETE FROM characterAttribute WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            ExecuteNonQuery("DELETE FROM characterSkill WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            ExecuteNonQuery("DELETE FROM characterBuff WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            ExecuteNonQuery("DELETE FROM characterHotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            ExecuteNonQuery("DELETE FROM characterQuest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }

        public override long FindCharacterName(string characterName)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM character WHERE characterName=@characterName",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }
    }
}
