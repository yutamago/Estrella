using System;
using System.IO;

namespace Estrella.FiestaLib.SHN
{
    public sealed class SHNWriter : BinaryWriter
    {
        public SHNWriter(Stream input)
            : base(input)
        {
        }

        public void WritePaddedString(string value, int Length)
        {
            var data = SHNFile.Encoding.GetBytes(value);
            if (data.Length > Length)
            {
                throw new ArgumentOutOfRangeException("Padded string is too long");
            }

            Write(data);
            Fill(0, Length - data.Length);
        }

        public void Fill(byte value, int count)
        {
            for (var i = 0; i < count; ++i)
            {
                Write(value);
            }
        }
    }
}