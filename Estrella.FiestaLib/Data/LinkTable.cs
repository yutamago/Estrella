using System;
using System.Data;

namespace Estrella.FiestaLib.Data
{
    public sealed class LinkTable
    {
        public string argument { get; private set; }
        public string MapServer { get; private set; }
        public string MapClient { get; private set; }
        public int CoordX { get; private set; }
        public int CoordY { get; private set; }
        public short Direct { get; private set; }
        public short LevelFrom { get; private set; }
        public short LevelTo { get; private set; }
        public byte Party { get; private set; }

        public static LinkTable Load(DataRow row)
        {
            var info = new LinkTable
            {
                argument = (string) row["argument"],
                MapServer = (string) row["MapServer"],
                MapClient = (string) row["MapClient"],
                CoordX = (int) row["Coord_X"],
                CoordY = (int) row["Coord_Y"],
                Direct = (short) (int) row["Direct"],
                LevelFrom = (short) (int) row["LevelFrom"],
                LevelTo = (short) (int) row["LevelTo"],
                Party = (byte) (sbyte) row["Party"]
            };
            return info;
        }
    }
}