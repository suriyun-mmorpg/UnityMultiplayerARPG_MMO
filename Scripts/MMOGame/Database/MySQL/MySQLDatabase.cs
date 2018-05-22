using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public class MySQLRowsReader
    {
        private readonly List<List<object>> data = new List<List<object>>();
        private readonly List<Dictionary<string, object>> dataDict = new List<Dictionary<string, object>>();
        private int currentRow = -1;
        public int FieldCount { get; private set; }
        public int VisibleFieldCount { get; private set; }
        public int RowCount { get { return data.Count; } }
        public bool HasRows { get { return RowCount > 0; } }

        public void Init(MySqlDataReader dataReader)
        {
            data.Clear();
            dataDict.Clear();
            FieldCount = dataReader.FieldCount;
            VisibleFieldCount = dataReader.VisibleFieldCount;
            while (dataReader.Read())
            {
                var row = new List<object>();
                var rowDict = new Dictionary<string, object>();
                for (var i = 0; i < FieldCount; ++i)
                {
                    var fieldName = dataReader.GetName(i);
                    var value = dataReader.GetValue(i);
                    row.Add(value);
                    rowDict.Add(fieldName, value);
                }
                data.Add(row);
                dataDict.Add(rowDict);
            }
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

        public byte GetByte(int index)
        {
            return (byte)data[currentRow][index];
        }

        public byte GetByte(string columnName)
        {
            return (byte)dataDict[currentRow][columnName];
        }

        public char GetChar(int index)
        {
            return (char)data[currentRow][index];
        }

        public char GetChar(string columnName)
        {
            return (char)dataDict[currentRow][columnName];
        }

        public string GetString(int index)
        {
            return (string)data[currentRow][index];
        }

        public string GetString(string columnName)
        {
            return (string)dataDict[currentRow][columnName];
        }

        public bool GetBoolean(int index)
        {
            return (bool)data[currentRow][index];
        }

        public bool GetBoolean(string columnName)
        {
            return (bool)dataDict[currentRow][columnName];
        }

        public short GetInt16(int index)
        {
            return (short)data[currentRow][index];
        }

        public short GetInt16(string columnName)
        {
            return (short)dataDict[currentRow][columnName];
        }

        public int GetInt32(int index)
        {
            return (int)data[currentRow][index];
        }

        public int GetInt32(string columnName)
        {
            return (int)dataDict[currentRow][columnName];
        }

        public long GetInt64(int index)
        {
            return (long)data[currentRow][index];
        }

        public long GetInt64(string columnName)
        {
            return (long)dataDict[currentRow][columnName];
        }

        public ushort GetUInt16(int index)
        {
            return (ushort)data[currentRow][index];
        }

        public ushort GetUInt16(string columnName)
        {
            return (ushort)dataDict[currentRow][columnName];
        }

        public uint GetUInt32(int index)
        {
            return (uint)data[currentRow][index];
        }

        public uint GetUInt32(string columnName)
        {
            return (uint)dataDict[currentRow][columnName];
        }

        public ulong GetUInt64(int index)
        {
            return (ulong)data[currentRow][index];
        }

        public ulong GetUInt64(string columnName)
        {
            return (ulong)dataDict[currentRow][columnName];
        }

        public decimal GetDecimal(int index)
        {
            return (decimal)data[currentRow][index];
        }

        public decimal GetDecimal(string columnName)
        {
            return (decimal)dataDict[currentRow][columnName];
        }

        public float GetFloat(int index)
        {
            return (float)data[currentRow][index];
        }

        public float GetFloat(string columnName)
        {
            return (float)dataDict[currentRow][columnName];
        }

        public double GetDouble(int index)
        {
            return (double)data[currentRow][index];
        }

        public double GetDouble(string columnName)
        {
            return (double)dataDict[currentRow][columnName];
        }

        public void ResetReader()
        {
            currentRow = -1;
        }
    }

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

        public override CharacterAttribute ReadCharacterAttribute(string characterId, string attributeId)
        {
            var reader = ExecuteReader("SELECT amount FROM characterAttribute WHERE characterId=@characterId AND attributeId=@attributeId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@attributeId", attributeId));
            if (reader.Read())
            {
                var result = new CharacterAttribute();
                result.attributeId = attributeId;
                result.amount = reader.GetInt32(0);
                return result;
            }
            return CharacterAttribute.Empty;
        }

        public override List<CharacterAttribute> ReadCharacterAttributes(string characterId)
        {
            throw new System.NotImplementedException();
        }

        public override CharacterSkill ReadCharacterSkill(string characterId, string skillId)
        {
            var reader = ExecuteReader("SELECT level, coolDownRemainsDuration FROM characterSkill WHERE characterId=@characterId AND skillId=@skillId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", skillId));
            if (reader.Read())
            {
                var result = new CharacterSkill();
                result.skillId = skillId;
                result.level = reader.GetInt32(0);
                result.coolDownRemainsDuration = reader.GetFloat(1);
                return result;
            }
            return CharacterSkill.Empty;
        }

        public override List<CharacterSkill> ReadCharacterSkills(string characterId)
        {
            throw new System.NotImplementedException();
        }

        public override CharacterBuff ReadCharacterBuff(string id)
        {
            var reader = ExecuteReader("SELECT characterId, dataId, type, level, buffRemainsDuration FROM characterBuff WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            if (reader.Read())
            {
                var result = new CharacterBuff();
                result.characterId = reader.GetInt64(0).ToString();
                result.dataId = reader.GetString(1);
                result.type = (BuffType)reader.GetByte(2);
                result.level = reader.GetInt32(3);
                result.buffRemainsDuration = reader.GetFloat(4);
                return result;
            }
            return CharacterBuff.Empty;
        }

        public override List<CharacterBuff> ReadCharacterBuffs(string characterId)
        {
            throw new System.NotImplementedException();
        }

        public override CharacterHotkey ReadCharacterHotkey(string characterId, string hotkeyId)
        {
            var reader = ExecuteReader("SELECT type, dataId FROM characterHotkey WHERE characterId=@characterId AND hotkeyId=@hotkeyId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@hotkeyId", hotkeyId));
            if (reader.Read())
            {
                var result = new CharacterHotkey();
                result.hotkeyId = hotkeyId;
                result.type = (HotkeyType)reader.GetByte(0);
                result.dataId = reader.GetString(1);
                return result;
            }
            return CharacterHotkey.Empty;
        }

        public override List<CharacterHotkey> ReadCharacterHotkeys(string characterId)
        {
            var result = new List<CharacterHotkey>();
            var reader = ExecuteReader("SELECT hotkeyId FROM characterHotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            while (reader.Read())
            {
                var hotkeyId = reader.GetString(0);
                result.Add(ReadCharacterHotkey(characterId, hotkeyId));
            }
            return result;
        }

        public override CharacterQuest ReadCharacterQuest(string characterId, string questId)
        {
            var reader = ExecuteReader("SELECT isComplete, killMonsters FROM characterQuest WHERE characterId=@characterId AND questId=@questId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@questId", questId));
            if (reader.Read())
            {
                var result = new CharacterQuest();
                result.questId = questId;
                result.isComplete = reader.GetBoolean(0);
                var killMonsters = reader.GetString(1);
                var killMonstersDict = new Dictionary<string, int>();
                var splitSets = killMonsters.Split(';');
                foreach (var set in splitSets)
                {
                    var splitData = set.Split(':');
                    killMonstersDict[splitData[0]] = int.Parse(splitData[1]);
                }
                result.killedMonsters = killMonstersDict;
                return result;
            }
            return CharacterQuest.Empty;
        }

        public override List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            var result = new List<CharacterQuest>();
            var reader = ExecuteReader("SELECT questId FROM characterQuest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            while (reader.Read())
            {
                var questId = reader.GetString(0);
                result.Add(ReadCharacterQuest(characterId, questId));
            }
            return result;
        }
    }
}
