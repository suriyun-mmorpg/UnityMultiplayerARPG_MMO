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
            return (byte)(long)data[currentRow][index];
        }

        public byte GetByte(string columnName)
        {
            return (byte)(long)dataDict[currentRow][columnName];
        }

        public sbyte GetSByte(int index)
        {
            return (sbyte)(long)data[currentRow][index];
        }

        public sbyte GetSByte(string columnName)
        {
            return (sbyte)(long)dataDict[currentRow][columnName];
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
            return ((long)data[currentRow][index]) != 0;
        }

        public bool GetBoolean(string columnName)
        {
            return ((long)dataDict[currentRow][columnName]) != 0;
        }

        public short GetInt16(int index)
        {
            try { return (short)(long)data[currentRow][index]; } catch { return 0; }
        }

        public short GetInt16(string columnName)
        {
            try { return (short)(long)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public int GetInt32(int index)
        {
            try { return (int)(long)data[currentRow][index]; } catch { return 0; }
        }

        public int GetInt32(string columnName)
        {
            try { return (int)(long)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public long GetInt64(int index)
        {
            try { return (long)data[currentRow][index]; } catch { return 0; }
        }

        public long GetInt64(string columnName)
        {
            try { return (long)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public ushort GetUInt16(int index)
        {
            try { return (ushort)(long)data[currentRow][index]; } catch { return 0; }
        }

        public ushort GetUInt16(string columnName)
        {
            try { return (ushort)(long)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public uint GetUInt32(int index)
        {
            try { return (uint)(long)data[currentRow][index]; } catch { return 0; }
        }

        public uint GetUInt32(string columnName)
        {
            try { return (uint)(long)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public ulong GetUInt64(int index)
        {
            try { return (ulong)(long)data[currentRow][index]; } catch { return 0; }
        }

        public ulong GetUInt64(string columnName)
        {
            try { return (ulong)(long)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public decimal GetDecimal(int index)
        {
            try { return (decimal)(float)data[currentRow][index]; } catch { return 0; }
        }

        public decimal GetDecimal(string columnName)
        {
            try { return (decimal)(float)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public float GetFloat(int index)
        {
            try { return (float)data[currentRow][index]; } catch { return 0; }
        }

        public float GetFloat(string columnName)
        {
            try { return (float)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public double GetDouble(int index)
        {
            try { return (float)data[currentRow][index]; } catch { return 0; }
        }

        public double GetDouble(string columnName)
        {
            try { return (float)dataDict[currentRow][columnName]; } catch { return 0; }
        }

        public void ResetReader()
        {
            currentRow = -1;
        }
    }
}
