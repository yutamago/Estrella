using System;
using System.Data;
using Estrella.Util;

namespace Estrella.FiestaLib.ShineTable
{
    public class ShineColumn : DataColumn
    {
        public ShineColumn(string name, string type, ref int unkColumns)
        {
            if (name.Length < 2)
            {
                name = "Unk_" + unkColumns++;
            }

            Caption = name;
            ColumnName = name;

            TypeName = type;
            DataType = GetColumnType();

            if (type.StartsWith("string["))
            {
                int from = type.IndexOf('['), till = type.IndexOf(']');
                var lenStr = type.Substring(from + 1, till - from - 1);
                var len = int.Parse(lenStr);
                MaxLength = len;
            }
        }

        public string TypeName { get; private set; }


        public Type GetColumnType()
        {
            var typeName = TypeName.ToLower();
            if (typeName.StartsWith("string["))
            {
                return typeof(string); // Char array actually ;p
            }

            switch (typeName)
            {
                case "byte": return typeof(byte);

                case "word": return typeof(short);

                case "<integer>":
                case "dwrd":
                case "dword": return typeof(int);

                case "qword": return typeof(long);

                case "index": return typeof(string);

                case "<string>":
                case "string": return typeof(string);
                default:
                    Log.WriteLine(LogLevel.Info, "Unknown column type found: {0} : {1}", typeName, ColumnName);
                    break;
            }

            return typeof(string); // Just to be sure ?!?! D:
        }
    }
}