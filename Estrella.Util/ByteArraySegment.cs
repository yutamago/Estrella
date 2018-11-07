using System;

namespace Estrella.Util
{
    public sealed class ByteArraySegment
    {
        public ByteArraySegment(byte[] pBuffer)
        {
            Start = 0;
            Buffer = pBuffer;
            Length = Buffer.Length;
        }

        public ByteArraySegment(byte[] pBuffer, int pStart, int pLength)
        {
            if (pStart + pLength > pBuffer.Length)
            {
                throw new ArgumentOutOfRangeException("pLength", "The segment doesn't fit the array bounds.");
            }

            Buffer = pBuffer;
            Start = pStart;
            Length = pLength;
        }

        public byte[] Buffer { get; private set; }
        public int Start { get; private set; }
        public int Length { get; private set; }

        public bool Advance(int pLength)
        {
            Start += pLength;
            Length -= pLength;
            return Length <= 0;
        }
    }
}