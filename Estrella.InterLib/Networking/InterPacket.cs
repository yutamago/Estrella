using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Estrella.Util;

namespace Estrella.InterLib.Networking
{
    public sealed class InterPacket : IDisposable
    {
        private MemoryStream _memoryStream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public InterPacket()
        {
            _memoryStream = new MemoryStream();
            _writer = new BinaryWriter(_memoryStream);
        }

        public InterPacket(ushort pOpCode)
        {
            _memoryStream = new MemoryStream();
            _writer = new BinaryWriter(_memoryStream);
            OpCode = (InterHeader) pOpCode;
            WriteUShort(pOpCode);
        }

        public InterPacket(byte[] pData)
        {
            _memoryStream = new MemoryStream(pData);
            _reader = new BinaryReader(_memoryStream);

            ushort opCode;
            TryReadUShort(out opCode);
            OpCode = (InterHeader) opCode;
        }

        public InterPacket(InterHeader type) : this((ushort) type)
        {
        }

        public InterHeader OpCode { get; private set; }

        public int Length
        {
            get { return (int) _memoryStream.Length; }
        }

        public int Cursor
        {
            get { return (int) _memoryStream.Position; }
        }

        public int Remaining
        {
            get { return (int) (_memoryStream.Length - _memoryStream.Position); }
        }

        public void Dispose()
        {
            if (_writer != null) _writer.Close();
            if (_reader != null) _reader.Close();
            _memoryStream = null;
            _writer = null;
            _reader = null;
        }

        ~InterPacket()
        {
            Dispose();
        }

        public void Seek(int offset)
        {
            if (offset > Length) throw new IndexOutOfRangeException("Cannot go to packet offset.");
            _memoryStream.Seek(offset, SeekOrigin.Begin);
        }

        public byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }

        public string Dump()
        {
            return ByteUtils.BytesToHex(_memoryStream.ToArray(),
                $"Packet (0x{OpCode.ToString("X4")} - {Length}): ");
        }

        public override string ToString()
        {
            var buf = new byte[Length - 2];
            Buffer.BlockCopy(_memoryStream.ToArray(), 2, buf, 0, buf.Length);
            return $"Opcode: 0x{(ushort) OpCode:X4} Length: {buf.Length} Data: {ByteUtils.BytesToHex(buf)}";
        }

        #region Write methods

        public void WriteHexAsBytes(string hexString)
        {
            var bytes = ByteUtils.HexToBytes(hexString);
            WriteBytes(bytes);
        }

        public void WriteDouble(double value)
        {
            _writer.Write(value);
        }

        public void WriteDateTime(DateTime value)
        {
            WriteLong(value.ToBinary());
        }

        public void SetByte(long pOffset, byte pValue)
        {
            var oldoffset = _memoryStream.Position;
            _memoryStream.Seek(pOffset, SeekOrigin.Begin);
            _writer.Write(pValue);
            _memoryStream.Seek(oldoffset, SeekOrigin.Begin);
        }

        public void Fill(int pLength, byte pValue)
        {
            for (var i = 0; i < pLength; ++i)
            {
                WriteByte(pValue);
            }
        }

        public void WriteBool(bool pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteByte(byte pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteSByte(sbyte pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteBytes(byte[] pBytes)
        {
            _writer.Write(pBytes);
        }

        public void WriteUShort(ushort pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteShort(short pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteUInt(uint pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteInt(int pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteFloat(float pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteULong(ulong pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteLong(long pValue)
        {
            _writer.Write(pValue);
        }

        public void WriteStringLen(string pValue, bool addNullTerminator = false)
        {
            if (addNullTerminator) pValue += char.MinValue;
            if (pValue.Length > 0xFF)
            {
                throw new Exception("Too long!");
            }

            WriteByte((byte) pValue.Length);
            WriteBytes(Encoding.ASCII.GetBytes(pValue));
            // NOTE: Some messages might be NULL terminated!
        }

        public void WriteString(string pValue)
        {
            WriteBytes(Encoding.ASCII.GetBytes(pValue));
            // NOTE: Some messages might be NULL terminated!
        }

        public void WriteString(string pValue, int pLen)
        {
            var buffer = Encoding.ASCII.GetBytes(pValue);
            if (buffer.Length > pLen)
            {
                throw new ArgumentException("pValue is bigger than pLen", "pLen");
            }

            WriteBytes(buffer);
            for (var i = 0; i < pLen - buffer.Length; i++)
            {
                WriteByte(0);
            }
        }


        public void Write(object val)
        {
            if (val is byte) WriteByte((byte) val);
            else if (val is sbyte) WriteSByte((sbyte) val);
            else if (val is byte[]) WriteBytes((byte[]) val);
            else if (val is short) WriteShort((short) val);
            else if (val is ushort) WriteUShort((ushort) val);
            else if (val is int) WriteInt((int) val);
            else if (val is uint) WriteUInt((uint) val);
            else if (val is long) WriteLong((long) val);
            else if (val is ulong) WriteULong((ulong) val);
            else if (val is double) WriteDouble((double) val);
            else if (val is float) WriteFloat((float) val);
            else if (val is string) WriteStringLen((string) val);
            else if (val is List<ushort>)
            {
                var list = val as List<ushort>;
                WriteInt(list.Count);
                foreach (var v in list) WriteUShort(v);
            }
            else if (val is List<int>)
            {
                var list = val as List<int>;
                WriteInt(list.Count);
                foreach (var v in list) WriteInt(v);
            }
            else
            {
                throw new Exception("Unknown type: " + val);
            }
        }

        #endregion

        #region Read methods

        public bool TryReadDouble(out double value)
        {
            value = 0;

            if (Remaining < 8)
                return false;

            value = _reader.ReadDouble();
            return true;
        }

        public bool ReadSkip(int pLength)
        {
            if (Remaining < pLength) return false;

            _memoryStream.Seek(pLength, SeekOrigin.Current);
            return true;
        }

        public bool TryReadDateTime(out DateTime value)
        {
            value = DateTime.MinValue;

            long data;
            if (Remaining < 8
                || !TryReadLong(out data))
                return false;

            value = DateTime.FromBinary(data);

            return true;
        }

        public bool TryReadBool(out bool pValue)
        {
            pValue = false;
            if (Remaining < 1) return false;
            pValue = _reader.ReadBoolean();
            return true;
        }

        public bool TryReadByte(out byte pValue)
        {
            pValue = 0;
            if (Remaining < 1) return false;
            pValue = _reader.ReadByte();
            return true;
        }

        public bool TryReadBytes(int pLength, out byte[] pValue)
        {
            pValue = new byte[] { };
            if (Remaining < pLength) return false;
            pValue = _reader.ReadBytes(pLength);
            return true;
        }

        public bool TryReadSByte(out sbyte pValue)
        {
            pValue = 0;
            if (Remaining < 1) return false;
            pValue = _reader.ReadSByte();
            return true;
        }

        // UInt16 is more conventional
        public bool TryReadUShort(out ushort pValue)
        {
            pValue = 0;
            if (Remaining < 2) return false;
            pValue = _reader.ReadUInt16();
            return true;
        }

        // Int16 is more conventional
        public bool TryReadShort(out short pValue)
        {
            pValue = 0;
            if (Remaining < 2) return false;
            pValue = _reader.ReadInt16();
            return true;
        }

        public bool TryReadFloat(out float pValue)
        {
            pValue = 0;
            if (Remaining < 2) return false;
            pValue = _reader.ReadSingle();
            return true;
        }

        // UInt32 is better
        public bool TryReadUInt(out uint pValue)
        {
            pValue = 0;
            if (Remaining < 4) return false;
            pValue = _reader.ReadUInt32();
            return true;
        }

        // Int32
        public bool TryReadInt(out int pValue)
        {
            pValue = 0;
            if (Remaining < 4) return false;
            pValue = _reader.ReadInt32();
            return true;
        }

        // UInt64
        public bool TryReadULong(out ulong pValue)
        {
            pValue = 0;
            if (Remaining < 8) return false;
            pValue = _reader.ReadUInt64();
            return true;
        }

        // Int64
        public bool TryReadLong(out long pValue)
        {
            pValue = 0;
            if (Remaining < 8) return false;
            pValue = _reader.ReadInt64();
            return true;
        }

        public bool TryReadString(out string pValue)
        {
            pValue = "";
            if (Remaining < 1) return false;
            byte len;
            TryReadByte(out len);
            if (Remaining < len) return false;
            return TryReadString(out pValue, len);
        }

        public bool TryReadString(out string pValue, int pLen)
        {
            pValue = "";
            if (Remaining < pLen) return false;

            var buffer = new byte[pLen];
            ReadBytes(buffer);
            var length = 0;
            if (buffer[pLen - 1] != 0)
            {
                length = pLen;
            }
            else
            {
                while (buffer[length] != 0x00 && length < pLen)
                {
                    length++;
                }
            }

            if (length > 0)
            {
                pValue = Encoding.ASCII.GetString(buffer, 0, length);
            }

            return true;
        }

        public bool ReadBytes(byte[] pBuffer)
        {
            if (Remaining < pBuffer.Length) return false;
            _memoryStream.Read(pBuffer, 0, pBuffer.Length);
            return true;
        }

        #endregion
    }
}