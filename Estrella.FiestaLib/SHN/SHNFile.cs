using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Estrella.FiestaLib.Encryption;

namespace Estrella.FiestaLib.SHN
{
    public delegate void DOnSaveFinished(SHNFile file);

    public delegate void DOnSaveError(SHNFile file, string error);

    public class SHNFile : DataTable
    {
        public static Encoding Encoding = Encoding.UTF8;
        private byte[] CryptHeader;

        private bool isSaving;

        public SHNFile(string pPath)
        {
            FileName = pPath;
            Load();
        }

        public SHNFile()
        {
        }

        public string FileName { get; private set; }
        public uint Header { get; private set; }
        public uint RecordCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint DefaultRecordLength { get; private set; }

        public event DOnSaveError OnSaveError;
        public event DOnSaveFinished OnSaveFinished;

        public void Load()
        {
            if (!File.Exists(FileName))
            {
                throw new FileNotFoundException($"Could not find SHN File {FileName}.");
            }

            byte[] data;
            using (var file = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new BinaryReader(file);
                CryptHeader = reader.ReadBytes(32);
                var Length = reader.ReadInt32() - 36; //minus int + header
                data = reader.ReadBytes(Length);
                FileCrypto.Crypt(data, 0, Length);
                //File.WriteAllBytes("Output.dat", data); //debug purpose
            }

            using (var stream = new MemoryStream(data))
            {
                var reader = new SHNReader(stream);
                Header = reader.ReadUInt32();
                RecordCount = reader.ReadUInt32();
                DefaultRecordLength = reader.ReadUInt32();
                ColumnCount = reader.ReadUInt32();
                GenerateColumns(reader);
                GenerateRows(reader);
            }

            data = null;
        }

        public bool Save(string path)
        {
            if (isSaving) return false;
            new Thread(delegate() { InternalSave(path); }).Start();
            return true;
        }

        private void InternalSave(string path)
        {
            try
            {
                isSaving = true;
                UpdateDefaultRecordLength();
                byte[] content;
                using (var encrypted = new MemoryStream())
                {
                    var writer = new SHNWriter(encrypted);
                    writer.Write(Header);
                    writer.Write((uint) Rows.Count);
                    writer.Write(DefaultRecordLength);
                    writer.Write((uint) Columns.Count);
                    WriteColumns(writer);
                    WriteRows(writer);
                    content = new byte[encrypted.Length];
                    encrypted.Seek(0, SeekOrigin.Begin);
                    encrypted.Read(content, 0, content.Length);
                }

                FileCrypto.Crypt(content, 0, content.Length);
                using (var final = File.Create(path))
                {
                    var writer = new BinaryWriter(final);
                    writer.Write(CryptHeader);
                    writer.Write(content.Length + 36);
                    writer.Write(content);
                }

                FileName = path;
                OnSaveFinished?.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + ex.StackTrace);
                OnSaveError?.Invoke(this, ex.ToString());
            }
            finally
            {
                isSaving = false;
            }
        }

        private void WriteRows(SHNWriter writer)
        {
            foreach (DataRow row in Rows)
            {
                var CurPos = (int) writer.BaseStream.Position;
                short unkLength = 0;
                writer.Write((short) 0); // Row Length
                for (var colIndex = 0; colIndex < Columns.Count; ++colIndex)
                {
                    var column = (ShnColumn) Columns[colIndex];
                    switch (column.TypeByte)
                    {
                        case 1:
                        case 12:
                        case 16:
                            writer.Write((byte) row[colIndex]);
                            break;
                        case 2:
                            writer.Write((ushort) row[colIndex]);
                            break;
                        case 3:
                        case 11:
                        case 18:
                        case 27:
                            writer.Write((uint) row[colIndex]);
                            break;
                        case 5:
                            writer.Write((float) row[colIndex]);
                            break;
                        case 9:
                        case 24:
                            writer.WritePaddedString((string) row[colIndex], column.Length);
                            break;
                        case 13:
                        case 21:
                            writer.Write((short) row[colIndex]);
                            break;
                        case 20:
                            writer.Write((sbyte) row[colIndex]);
                            break;
                        case 22:
                            writer.Write((int) row[colIndex]);
                            break;
                        case 26:
                            var tmp = (string) row[colIndex];
                            unkLength += (short) tmp.Length;
                            writer.WritePaddedString(tmp, tmp.Length + 1);
                            break;
                    }
                }

                var LastPos = (int) writer.BaseStream.Position;
                writer.Seek(CurPos, SeekOrigin.Begin);
                writer.Write((short) (DefaultRecordLength + unkLength)); // Update Row Length
                writer.Seek(LastPos, SeekOrigin.Begin);
            }
        }

        private void WriteColumns(SHNWriter writer)
        {
            for (var i = 0; i < Columns.Count; ++i)
            {
                ((ShnColumn) Columns[i]).Write(writer);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) CryptHeader = null;
            base.Dispose(disposing);
        }

        private void UpdateDefaultRecordLength()
        {
            uint len = 2;
            for (var i = 0; i < Columns.Count; ++i)
            {
                var col = (ShnColumn) Columns[i];
                len += (uint) col.Length;
            }

            DefaultRecordLength = len;
        }

        public void GenerateRows(SHNReader reader)
        {
            var values = new object[ColumnCount];
            for (uint i = 0; i < RecordCount; ++i)
            {
                uint RowLength = reader.ReadUInt16();
                for (var j = 0; j < ColumnCount; ++j)
                {
                    switch (((ShnColumn) Columns[j]).TypeByte)
                    {
                        case 1:
                        case 12:
                        case 16:
                            values[j] = reader.ReadByte();
                            break;
                        case 2:
                            values[j] = reader.ReadUInt16();
                            break;
                        case 3:
                        case 11:
                        case 18:
                        case 27:
                            values[j] = reader.ReadUInt32();
                            break;
                        case 5:
                            values[j] = reader.ReadSingle();
                            break;
                        case 9:
                        case 24:
                            values[j] = reader.ReadPaddedString(((ShnColumn) Columns[j]).Length);
                            break;
                        case 13:
                        case 21:
                            values[j] = reader.ReadInt16();
                            break;
                        case 20:
                            values[j] = reader.ReadSByte();
                            break;
                        case 22:
                            values[j] = reader.ReadInt32();
                            break;
                        case 26
                            : // TODO: Should be read until first null byte, to support more than 1 this kind of column
                            values[j] = reader.ReadPaddedString((int) (RowLength - DefaultRecordLength + 1));
                            break;
                        default:
                            throw new Exception("New column type found");
                    }
                }

                Rows.Add(values);
            }
        }

        public void GenerateColumns(SHNReader reader)
        {
            var unkcolumns = 0;
            var Length = 2;
            for (var i = 0; i < ColumnCount; ++i)
            {
                var col = new ShnColumn();
                col.Load(reader, ref unkcolumns);
                Length += col.Length;
                Columns.Add(col);
            }

            if (Length != DefaultRecordLength)
            {
                throw new Exception("Default record Length does not fit.");
            }
        }
    }
}