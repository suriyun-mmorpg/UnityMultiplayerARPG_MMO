using System.Collections.Generic;
using System.Data.Common;

namespace Insthync.MMOG
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

        public System.DateTime GetDateTime(string columnName)
        {
            return (System.DateTime)dataDict[currentRow][columnName];
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
}
