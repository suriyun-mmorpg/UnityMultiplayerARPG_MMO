using System;
using System.Collections.Generic;
using System.Data.Common;

namespace MultiplayerARPG.MMO
{
    public class SQLiteRowsReader
    {
        private readonly List<List<object>> data = new List<List<object>>();
        private readonly List<Dictionary<string, object>> dataDict = new List<Dictionary<string, object>>();
        private int currentRow = -1;
        public int FieldCount { get; private set; }
        public int VisibleFieldCount { get; private set; }
        public int RowCount { get { return data.Count; } }
        public bool HasRows { get { return RowCount > 0; } }

        public void Init(DbDataReader dataReader)
        {
            data.Clear();
            dataDict.Clear();
            FieldCount = dataReader.FieldCount;
            VisibleFieldCount = dataReader.VisibleFieldCount;
            while (dataReader.Read())
            {
                List<object> row = new List<object>();
                Dictionary<string, object> rowDict = new Dictionary<string, object>();
                for (int i = 0; i < FieldCount; ++i)
                {
                    string fieldName = dataReader.GetName(i);
                    object value = dataReader.GetValue(i);
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

        public DateTime GetDateTime(int index)
        {
            return (DateTime)data[currentRow][index];
        }

        public DateTime GetDateTime(string columnName)
        {
            return (DateTime)dataDict[currentRow][columnName];
        }

        public object GetObject(int index)
        {
            return data[currentRow][index];
        }

        public object GetObject(string columnName)
        {
            return dataDict[currentRow][columnName];
        }

        public byte GetByte(int index)
        {
            return Convert.ToByte(data[currentRow][index]);
        }

        public byte GetByte(string columnName)
        {
            return Convert.ToByte(dataDict[currentRow][columnName]);
        }

        public sbyte GetSByte(int index)
        {
            return Convert.ToSByte(data[currentRow][index]);
        }

        public sbyte GetSByte(string columnName)
        {
            return Convert.ToSByte(dataDict[currentRow][columnName]);
        }

        public char GetChar(int index)
        {
            return Convert.ToChar(data[currentRow][index]);
        }

        public char GetChar(string columnName)
        {
            return Convert.ToChar(dataDict[currentRow][columnName]);
        }

        public string GetString(int index)
        {
            return Convert.ToString(data[currentRow][index]);
        }

        public string GetString(string columnName)
        {
            return Convert.ToString(dataDict[currentRow][columnName]);
        }

        public bool GetBoolean(int index)
        {
            return Convert.ToBoolean(data[currentRow][index]);
        }

        public bool GetBoolean(string columnName)
        {
            return Convert.ToBoolean(dataDict[currentRow][columnName]);
        }

        public short GetInt16(int index)
        {
            return Convert.ToInt16(data[currentRow][index]);
        }

        public short GetInt16(string columnName)
        {
            return Convert.ToInt16(dataDict[currentRow][columnName]);
        }

        public int GetInt32(int index)
        {
            return Convert.ToInt32(data[currentRow][index]);
        }

        public int GetInt32(string columnName)
        {
            return Convert.ToInt32(dataDict[currentRow][columnName]);
        }

        public long GetInt64(int index)
        {
            return Convert.ToInt64(data[currentRow][index]);
        }

        public long GetInt64(string columnName)
        {
            return Convert.ToInt64(dataDict[currentRow][columnName]);
        }

        public ushort GetUInt16(int index)
        {
            return Convert.ToUInt16(data[currentRow][index]);
        }

        public ushort GetUInt16(string columnName)
        {
            return Convert.ToUInt16(dataDict[currentRow][columnName]);
        }

        public uint GetUInt32(int index)
        {
            return Convert.ToUInt32(data[currentRow][index]);
        }

        public uint GetUInt32(string columnName)
        {
            return Convert.ToUInt32(dataDict[currentRow][columnName]);
        }

        public ulong GetUInt64(int index)
        {
            return Convert.ToUInt64(data[currentRow][index]);
        }

        public ulong GetUInt64(string columnName)
        {
            return Convert.ToUInt64(dataDict[currentRow][columnName]);
        }

        public decimal GetDecimal(int index)
        {
            return Convert.ToDecimal(data[currentRow][index]);
        }

        public decimal GetDecimal(string columnName)
        {
            return Convert.ToDecimal(dataDict[currentRow][columnName]);
        }

        public float GetFloat(int index)
        {
            return Convert.ToSingle(data[currentRow][index]);
        }

        public float GetFloat(string columnName)
        {
            return Convert.ToSingle(dataDict[currentRow][columnName]);
        }

        public double GetDouble(int index)
        {
            return Convert.ToDouble(data[currentRow][index]);
        }

        public double GetDouble(string columnName)
        {
            return Convert.ToDouble(dataDict[currentRow][columnName]);
        }

        public void ResetReader()
        {
            currentRow = -1;
        }
    }
}
