using System;
using System.Data;

namespace Estrella.FiestaLib.SHN
{
    public class ShnColumn : DataColumn
    {
        public int Length { get; private set; }
        public byte TypeByte { get; private set; }

        public void Load(SHNReader reader, ref int unkcount)
        {
            var caption = reader.ReadPaddedString(48);
            if (caption.Trim().Length < 2)
            {
                ColumnName = "Undefined " + unkcount;
                ++unkcount;
            }
            else
            {
                ColumnName = caption;
            }

            TypeByte = (byte) reader.ReadUInt32();
            DataType = GetType(TypeByte);
            Length = reader.ReadInt32();
        }

        public void Write(SHNWriter writer)
        {
            if (ColumnName.StartsWith("Undefined"))
            {
                writer.WritePaddedString(" ", 48);
            }
            else
            {
                writer.WritePaddedString(ColumnName, 48);
            }

            writer.Write((uint) TypeByte);
            writer.Write((uint) Length);
        }

        public static Type GetType(uint pCode)
        {
            switch (pCode)
            {
                case 1:
                case 12:
                    return typeof(byte);
                case 2:
                    return typeof(ushort);
                case 3:
                case 11:
                    return typeof(uint);
                case 5:
                    return typeof(float);
                case 0x15:
                case 13:
                    return typeof(short);
                case 0x10:
                    return typeof(byte);
                case 0x12:
                case 0x1b:
                    return typeof(uint);
                case 20:
                    return typeof(sbyte);
                case 0x16:
                    return typeof(int);
                case 0x18:
                case 0x1a:
                case 9:
                    return typeof(string);
                default:
                    return typeof(object);
            }
        }
    }
}