using System.Data;
using System.IO;

namespace Estrella.FiestaLib.ShineTable
{
    public class ShineTable : DataTable
    {
        private int unkColumns;

        public ShineTable(string name)
        {
            TableName = name;
        }

        public void AddColumn(string name, string type)
        {
            Columns.Add(new ShineColumn(name, type, ref unkColumns));
        }

        public void AddRow(object[] data)
        {
            Rows.Add(data);
        }

        public void ToFile(string filename)
        {
            using (var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var stream = new StreamWriter(file))
                {
                    stream.WriteLine("using System;");
                    stream.WriteLine("using Estrella.Util;");

                    stream.WriteLine();
                    stream.WriteLine("namespace YourNamespaceHere");
                    stream.WriteLine("{");
                    stream.WriteLine("\tpublic sealed class {0}", TableName);
                    stream.WriteLine("\t{");

                    foreach (ShineColumn column in Columns)
                    {
                        stream.WriteLine("\t\tpublic {0} {1} {{ get; private set; }}", column.GetColumnType().Name, column.ColumnName);
                    }

                    stream.WriteLine();

                    stream.WriteLine("\t\tpublic static {0} Load(DataTableReaderEx reader)", TableName);
                    stream.WriteLine("\t\t{");
                    stream.WriteLine("\t\t\t{0} info = new {0}", TableName);
                    stream.WriteLine("\t\t\t{");
                    foreach (ShineColumn column in Columns)
                    {
                        stream.WriteLine("\t\t\t\t{0} = reader.Get{1}(\"{0}\"),", column.ColumnName,
                            column.GetColumnType().Name);
                    }

                    stream.WriteLine("\t\t\t};");
                    stream.WriteLine("\t\t\treturn info;");
                    stream.WriteLine("\t\t}");


                    stream.WriteLine("\t}");
                    stream.WriteLine("}");
                }
            }
        }
    }
}