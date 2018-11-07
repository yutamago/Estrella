using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Estrella.FiestaLib.ShineTable
{
    public class ShineReader : IDisposable
    {
        const string Comment = ";";
        const string StructInteger = "<integer>";
        const string StructString = "<string>";

        const string Replace = "#exchange";
        const string Ignore = "#ignore";
        const string StartDefine = "#define";
        const string EndDefine = "#enddefine";
        const string Table = "#table";
        const string ColumnName = "#columnname";
        const string ColumnType = "#columntype";
        const string Record = "#record";
        const string RecordLine = "#recordin"; // Contains tablename as first row.
        private bool isDisposed;

        public ShineReader(string filename)
        {
            // Load up the shizz
            ParseShineTable(filename);
        }

        public Dictionary<string, ShineTable> FileContents { get; private set; }

        public static string PStructString
        {
            get { return StructString; }
        }

        public static string PStructInteger
        {
            get { return StructInteger; }
        }

        public ShineTable this[string index]
        {
            get { return FileContents[index]; }
        }

        public void Dispose()
        {
            if (!isDisposed && FileContents != null)
            {
                foreach (var kvp in FileContents.Values)
                {
                    kvp.Dispose();
                }

                FileContents.Clear();
                isDisposed = true;
            }
        }

        ~ShineReader()
        {
            Dispose();
        }

        public void ParseShineTable(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException(file);

            FileContents = new Dictionary<string, ShineTable>();

            using (var files = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(files, Encoding.Default))
                {
                    bool definingStruct = false, definingTable = false;
                    var lineNR = 0;
                    ShineTable curTable = null;
                    List<string> columnTypes = null;
                    var comment = "";
                    char? ignore = null;
                    KeyValuePair<string, string>? replaceThis = null;
                    while (!sr.EndOfStream)
                    {
                        lineNR++;
                        var line = sr.ReadLine().TrimStart();
                        if (line.Contains(Comment))
                        {
                            // Remove everything after it.
                            var index = line.IndexOf(Comment);
                            comment = line.Substring(index + 1);
                            //Console.WriteLine("Comment @ {0}: {1}", lineNR, comment);

                            line = line.Remove(index);
                        }

                        if (ignore.HasValue)
                        {
                            line = line.Replace(ignore.Value.ToString(), "");
                        }

                        if (line == string.Empty)
                        {
                            continue;
                        }

                        var lineLower = line.ToLower();
                        var lineSplit = line.Split('\t');

                        if (lineLower.StartsWith(Replace))
                        {
                            // ...
                            replaceThis = new KeyValuePair<string, string>(ConvertShitToString(lineSplit[1]),
                                ConvertShitToString(lineSplit[2])); // take risks :D
                            //continue;
                        }

                        if (lineLower.StartsWith(Ignore))
                        {
                            ignore = ConvertShitToString(lineSplit[1])[0];
                            //continue;
                        }

                        if (lineLower.StartsWith(StartDefine))
                        {
                            if (definingStruct || definingTable)
                            {
                                throw new Exception("Already defining.");
                            }

                            // Get the name..
                            var name = line.Substring(StartDefine.Length + 1);
                            curTable = new ShineTable(name);

                            definingStruct = true;
                            continue;
                        }

                        if (lineLower.StartsWith(Table))
                        {
                            if (definingStruct)
                            {
                                throw new Exception("Already defining.");
                            }

                            // Get the name..
                            var name = lineSplit[1].Trim(); // I hope this works D;
                            curTable = new ShineTable(name);
                            columnTypes = new List<string>();
                            FileContents.Add(name, curTable);
                            definingTable = true;
                            continue;
                        }

                        if (lineLower.StartsWith(EndDefine))
                        {
                            if (!definingStruct)
                            {
                                throw new Exception("Not started defining.");
                            }

                            definingStruct = false;
                            FileContents.Add(curTable.TableName, curTable);
                            continue;
                        }

                        line = line.Trim();
                        lineLower = lineLower.Trim();

                        if (definingStruct)
                        {
                            var columnName = comment.Trim();
                            if (columnName == string.Empty) continue;
                            curTable.AddColumn(columnName, lineLower);
                            Console.WriteLine("Added column {0} to table {1}", columnName, curTable.TableName);
                        }
                        else if (definingTable)
                        {
                            // Lets search for columns..
                            if (lineLower.StartsWith(ColumnType))
                            {
                                for (var i = 1; i < lineSplit.Length; i++)
                                {
                                    var l = lineSplit[i].Trim();
                                    if (l == string.Empty) continue;
                                    columnTypes.Add(l);
                                }
                            }
                            else if (lineLower.StartsWith(ColumnName))
                            {
                                var j = 0;
                                for (var i = 1; i < lineSplit.Length; i++)
                                {
                                    var l = lineSplit[i].Trim();
                                    if (l == string.Empty) continue;
                                    var coltype = columnTypes[j++];
                                    //curTable.AddColumn(l + "(" + coltype + ")", coltype);
                                    curTable.AddColumn(l, coltype);
                                }
                            }
                            else if (lineLower.StartsWith(RecordLine))
                            {
                                // Next column is tablename
                                var tablename = lineSplit[1].Trim();
                                if (FileContents.ContainsKey(tablename))
                                {
                                    curTable = FileContents[tablename];
                                    // Lets start.
                                    var data = new object[curTable.Columns.Count];
                                    var j = 0;
                                    for (var i = 2; i < lineSplit.Length; i++)
                                    {
                                        var l = lineSplit[i].Trim();
                                        if (l == string.Empty) continue;
                                        data[j++] = Check(replaceThis, l.TrimEnd(','));
                                    }

                                    curTable.AddRow(data);
                                }
                            }
                            else if (lineLower.StartsWith(Record))
                            {
                                var data = new object[curTable.Columns.Count];
                                // Right under the table
                                var j = 0;
                                for (var i = 1; i < lineSplit.Length; i++)
                                {
                                    if (j >= curTable.Columns.Count) break;
                                    var l = lineSplit[i].Trim();
                                    if (l == string.Empty) continue;
                                    data[j++] = Check(replaceThis, l.TrimEnd(','));
                                }

                                curTable.AddRow(data);
                            }
                        }
                        else
                        {
                            if (FileContents.ContainsKey(lineSplit[0].Trim()))
                            {
                                // Should be a struct I guess D:
                                var table = FileContents[lineSplit[0].Trim()];
                                var columnsInStruct = table.Columns.Count;
                                var readColumns = 0;
                                var data = new object[columnsInStruct];
                                for (var i = 1;; i++)
                                {
                                    if (readColumns == columnsInStruct)
                                    {
                                        break;
                                    }

                                    if (lineSplit.Length < i)
                                    {
                                        throw new Exception($"Could not read all columns of line {lineNR}");
                                    }

                                    // Cannot count on the tabs ...
                                    var columnText = lineSplit[i].Trim();
                                    if (columnText == string.Empty) continue;
                                    // Well, lets see if we can put it into the list
                                    columnText = columnText.TrimEnd(',').Trim('"');

                                    data[readColumns++] = columnText;
                                }

                                table.AddRow(data);
                            }
                        }
                    }
                }
            }
        }

        private static string ConvertShitToString(string input)
        {
            // HACKZ IN HERE
            if (input.StartsWith("\\x"))
            {
                return ((char) Convert.ToByte(input.Substring(2), 16)).ToString();
            }

            if (input.StartsWith("\\o"))
            {
                return ((char) Convert.ToByte(input.Substring(2), 8)).ToString();
            }

            return input.Length > 0 ? input[0].ToString() : "";
        }

        private static string Check(KeyValuePair<string, string>? replacing, string input)
        {
            return replacing.HasValue ? input.Replace(replacing.Value.Key, replacing.Value.Value) : input;
        }
    }
}