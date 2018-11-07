using System.IO;

namespace Estrella.FiestaLib.SHN
{
    public sealed class SHNReader : BinaryReader
    {
        public SHNReader(Stream input)
            : base(input)
        {
        }

        public string ReadPaddedString(int length)
        {
            var value = string.Empty;
            var offset = 0;
            var buffer = ReadBytes(length);
            while (offset < length && buffer[offset] != 0x00) offset++;
            if (length > 0) value = SHNFile.Encoding.GetString(buffer, 0, offset);
            return value;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public long Skip(long offset)
        {
            return Seek(offset, SeekOrigin.Current);
        }
    }
}