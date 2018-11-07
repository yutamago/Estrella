using System;
using System.IO;
using System.Text;
using Estrella.Util;

namespace Estrella.FiestaLib.Networking
{
    public sealed class Packet : IDisposable
    {
        private MemoryStream memoryStream;
        private BinaryReader reader;
        private BinaryWriter writer;

        public Packet()
        {
            memoryStream = new MemoryStream();
            writer = new BinaryWriter(memoryStream);
        }

        public Packet(ushort pOpCode)
        {
            memoryStream = new MemoryStream();
            writer = new BinaryWriter(memoryStream);
            Header = (byte) (pOpCode >> 10);
            Type = (byte) (pOpCode & 1023);
            OpCode = pOpCode;
            WriteUShort(pOpCode);
        }

        public Packet(byte pHeader, byte pType)
        {
            memoryStream = new MemoryStream();
            writer = new BinaryWriter(memoryStream);
            Header = pHeader;
            Type = pType;
            var realheader = (ushort) ((pHeader << 10) + (pType & 1023));
            OpCode = realheader;
            WriteUShort(realheader);
        }

        public Packet(byte[] pData)
        {
            memoryStream = new MemoryStream(pData);
            reader = new BinaryReader(memoryStream);

            ushort opCode;
            TryReadUShort(out opCode);
            Header = (byte) (opCode >> 10);
            Type = (byte) (opCode & 1023);
            OpCode = opCode;
        }

        public Packet(SH2Type type) : this(2, (byte) type)
        {
        }

        public Packet(SH3Type type) : this(3, (byte) type)
        {
        }

        public Packet(SH4Type type) : this(4, (byte) type)
        {
        }

        public Packet(SH5Type type) : this(5, (byte) type)
        {
        }

        public Packet(SH6Type type) : this(6, (byte) type)
        {
        }

        public Packet(SH7Type type) : this(7, (byte) type)
        {
        }

        public Packet(SH8Type type) : this(8, (byte) type)
        {
        }

        public Packet(SH9Type type) : this(9, (byte) type)
        {
        }

        public Packet(SH12Type type) : this(12, (byte) type)
        {
        }

        public Packet(SH15Type type) : this(15, (byte) type)
        {
        }

        public Packet(SH18Type type) : this(18, (byte) type)
        {
        }

        public Packet(SH19Type type) : this(19, (byte) type)
        {
        }

        public Packet(SH20Type type) : this(20, (byte) type)
        {
        }

        public Packet(SH21Type type) : this(21, (byte) type)
        {
        }

        public Packet(SH25Type type) : this(25, (byte) type)
        {
        }

        public Packet(SH28Type type) : this(28, (byte) type)
        {
        }

        public Packet(SH29Type type) : this(29, (byte) type)
        {
        }

        public Packet(SH31Type type) : this(31, (byte) type)
        {
        }

        public Packet(SH14Type type) : this(14, (byte) type)
        {
        }

        public Packet(SH37Type type) : this(37, (byte) type)
        {
        }

        public Packet(SH38Type type) : this(38, (byte) type)
        {
        }

        public Packet(SH42Type type) : this(42, (byte) type)
        {
        }

        public ushort OpCode { get; private set; }
        public byte Header { get; private set; } //new packet system
        public byte Type { get; private set; }

        public int Length
        {
            get { return (int) memoryStream.Length; }
        }

        public int Cursor
        {
            get { return (int) memoryStream.Position; }
        }

        public int Remaining
        {
            get { return (int) (memoryStream.Length - memoryStream.Position); }
        }

        public void Dispose()
        {
            if (writer != null) writer.Close();
            if (reader != null) reader.Close();
            memoryStream = null;
            writer = null;
            reader = null;
        }

        ~Packet()
        {
            Dispose();
        }

        public void Seek(int offset)
        {
            if (offset > Length) throw new IndexOutOfRangeException("Cannot go to packet offset.");
            memoryStream.Seek(offset, SeekOrigin.Begin);
        }

        public byte[] ToPacketArray()
        {
            //TODO: faster buffer copy
            byte[] buffer;
            var encbuffer = memoryStream.ToArray();
            if (encbuffer.Length <= 0xff)
            {
                buffer = new byte[encbuffer.Length + 1];
                Buffer.BlockCopy(encbuffer, 0, buffer, 1, encbuffer.Length);
                buffer[0] = (byte) encbuffer.Length;
            }
            else
            {
                buffer = new byte[encbuffer.Length + 3];
                Buffer.BlockCopy(encbuffer, 0, buffer, 3, encbuffer.Length);
                Buffer.BlockCopy(BitConverter.GetBytes((ushort) encbuffer.Length), 0, buffer, 1, 2);
            }

            return buffer;
        }

        public byte[] ToNormalArray()
        {
            return memoryStream.ToArray();
        }

        public string Dump()
        {
            return ByteUtils.BytesToHex(memoryStream.ToArray(),
                $"Packet (0x{OpCode.ToString("X4")} - {Length}): ");
        }

        public override string ToString()
        {
            var buf = new byte[Length - 2];
            Buffer.BlockCopy(memoryStream.ToArray(), 2, buf, 0, buf.Length);
            return $"{Header}|{Type} Opcode: 0x{OpCode:X4} Length: {buf.Length} Data: {ByteUtils.BytesToHex(buf)}";
        }

        #region Write methods

        public void Padding(int count)
        {
            for (var i = 0; i < count; i++) writer.Write((byte) 00);
        }


        public void FillPadding(string value, int count)
        {
            var padding = count - value.Length;
            foreach (var c in value) writer.Write((byte) c);
            Padding(padding);
        }

        public void WriteHexAsBytes(string hexString)
        {
            var bytes = ByteUtils.HexToBytes(hexString);
            WriteBytes(bytes);
        }

        public void SetByte(long pOffset, byte pValue)
        {
            var oldoffset = memoryStream.Position;
            memoryStream.Seek(pOffset, SeekOrigin.Begin);
            writer.Write(pValue);
            memoryStream.Seek(oldoffset, SeekOrigin.Begin);
        }

        public void Fill(int pLength, byte pValue)
        {
            for (var i = 0; i < pLength; ++i)
            {
                WriteByte(pValue);
            }
        }

        public void WriteDouble(double pValue)
        {
            writer.Write(pValue);
        }

        public void WriteBool(bool pValue)
        {
            writer.Write(pValue);
        }

        public void WriteByte(byte pValue)
        {
            writer.Write(pValue);
        }

        public void WriteSByte(sbyte pValue)
        {
            writer.Write(pValue);
        }

        public void WriteBytes(byte[] pBytes)
        {
            writer.Write(pBytes);
        }

        public void WriteUShort(ushort pValue)
        {
            writer.Write(pValue);
        }

        public void WriteShort(short pValue)
        {
            writer.Write(pValue);
        }

        public void WriteUInt(uint pValue)
        {
            writer.Write(pValue);
        }

        public void WriteInt(int pValue)
        {
            writer.Write(pValue);
        }

        public void WriteFloat(float pValue)
        {
            writer.Write(pValue);
        }

        public void WriteULong(ulong pValue)
        {
            writer.Write(pValue);
        }

        public void WriteLong(long pValue)
        {
            writer.Write(pValue);
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
                throw new ArgumentException("pValue is bigger than pLen", "pLen " + buffer.Length + "");
            }

            WriteBytes(buffer);
            for (var i = 0; i < pLen - buffer.Length; i++)
            {
                WriteByte(0);
            }
        }

        #endregion

        #region Read methods

        public bool ReadSkip(int pLength)
        {
            if (Remaining < pLength) return false;

            memoryStream.Seek(pLength, SeekOrigin.Current);
            return true;
        }

        public bool TryReadBool(out bool pValue)
        {
            pValue = false;
            if (Remaining < 1) return false;
            pValue = reader.ReadBoolean();
            return true;
        }

        public bool TryReadByte(out byte pValue)
        {
            pValue = 0;
            if (Remaining < 1) return false;
            pValue = reader.ReadByte();
            return true;
        }

        public bool TryReadBytes(int pLength, out byte[] pValue)
        {
            pValue = new byte[] { };
            if (Remaining < pLength) return false;
            pValue = reader.ReadBytes(pLength);
            return true;
        }

        public string ReadStringForLogin(int length)
        {
            string tempString = null;
            var stringRead = reader.ReadChars(length);
            foreach (var c in stringRead) tempString += c;
            return tempString;
        }

        public bool TryReadSByte(out sbyte pValue)
        {
            pValue = 0;
            if (Remaining < 1) return false;
            pValue = reader.ReadSByte();
            return true;
        }

        // UInt16 is more conventional
        public bool TryReadUShort(out ushort pValue)
        {
            pValue = 0;
            if (Remaining < 2) return false;
            pValue = reader.ReadUInt16();
            return true;
        }

        // Int16 is more conventional
        public bool TryReadShort(out short pValue)
        {
            pValue = 0;
            if (Remaining < 2) return false;
            pValue = reader.ReadInt16();
            return true;
        }

        public bool TryReadFloat(out float pValue)
        {
            pValue = 0;
            if (Remaining < 2) return false;
            pValue = reader.ReadSingle();
            return true;
        }

        // UInt32 is better
        public bool TryReadUInt(out uint pValue)
        {
            pValue = 0;
            if (Remaining < 4) return false;
            pValue = reader.ReadUInt32();
            return true;
        }

        // Int32
        public bool TryReadInt(out int pValue)
        {
            pValue = 0;
            if (Remaining < 4) return false;
            pValue = reader.ReadInt32();
            return true;
        }

        // UInt64
        public bool TryReadULong(out ulong pValue)
        {
            pValue = 0;
            if (Remaining < 8) return false;
            pValue = reader.ReadUInt64();
            return true;
        }

        // UInt64
        public bool TryReadLong(out long pValue)
        {
            pValue = 0;
            if (Remaining < 8) return false;
            pValue = reader.ReadInt64();
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
            memoryStream.Read(pBuffer, 0, pBuffer.Length);
            return true;
        }

        #endregion
    }
}