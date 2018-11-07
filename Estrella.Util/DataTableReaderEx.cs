using System;
using System.Collections.Generic;
using System.Data;

namespace Estrella.Util
{
    /// <summary>
    ///     Class to read a DataTable by ColumnName, as .NET one doesn't support this.
    /// </summary>
    public sealed class DataTableReaderEx : IDisposable
    {
        private readonly DataTableReader reader;
        private Dictionary<string, int> columns = new Dictionary<string, int>();
        private bool isDisposed;

        public DataTableReaderEx(DataTable pTable)
        {
            isDisposed = false;
            //Thank you MicroSoft, for making it a sealed class...
            reader = new DataTableReader(pTable);
            for (var i = 0; i < pTable.Columns.Count; ++i)
            {
                columns.Add(pTable.Columns[i].Caption.ToLower(), i);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                reader.Dispose();
                columns.Clear();
                columns = null;
                isDisposed = true;
            }
        }

        public bool Read()
        {
            return reader.Read();
        }

        public string GetString(string pColumnName)
        {
            return reader.GetString(GetIndex(pColumnName));
        }

        public int GetInt32(string pColumnName)
        {
            return reader.GetInt32(GetIndex(pColumnName));
        }

        public uint GetUInt32(string pColumnName)
        {
            return Convert.ToUInt32(reader.GetValue(GetIndex(pColumnName)));
        }

        public ulong GetUInt64(string pColumnName)
        {
            return Convert.ToUInt64(reader.GetValue(GetIndex(pColumnName)));
        }

        public ushort GetUInt16(string pColumnName)
        {
            //weird that this wasn't implemented at all
            return Convert.ToUInt16(reader.GetValue(GetIndex(pColumnName)));
        }

        public short GetInt16(string pColumnName)
        {
            return Convert.ToInt16(reader.GetValue(GetIndex(pColumnName)));
        }

        public byte GetByte(string pColumnName)
        {
            return Convert.ToByte(reader.GetValue(GetIndex(pColumnName)));
        }

        public float GetFloat(string pColumnName)
        {
            return Convert.ToSingle(reader.GetValue(GetIndex(pColumnName)));
        }

        public bool GetBoolean(string pColumnName)
        {
            return Convert.ToBoolean(reader.GetValue(GetIndex(pColumnName)));
        }

        public int GetIndex(string pName)
        {
            int offset;
            if (columns.TryGetValue(pName.ToLower(), out offset))
            {
                return offset;
            }

            throw new Exception("Column name not found: " + pName);
        }

        public Type GetType(string pName)
        {
            return reader.GetValue(GetIndex(pName)).GetType();
        }

        ~DataTableReaderEx()
        {
            Dispose();
        }
    }
}