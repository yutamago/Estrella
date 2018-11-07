using System;
using System.Data;

namespace Estrella.FiestaLib.Data
{
    public sealed class BlockInfo
    {
        private static readonly byte[] powers = {1, 2, 4, 8, 16, 32, 64, 128, 255};
        private int height;

        private int width;

        public BlockInfo(DataRow row, ushort mapId)
        {
            MapID = mapId;
            LoadBasics(row);
        }

        public ushort MapID { get; private set; }
        public string ShortName { get; private set; }

        private byte Read { get; set; }

        public int Width
        {
            get { return width * 50; }
        }

        public int Height
        {
            get { return (int) (height * 6.25); }
        }

        private void LoadBasics(DataRow row)
        {
            width = (int) row["Width"];
            height = (int) row["Height"];
            Read = (byte) (sbyte) row["Byte"];
        }

        public bool CanWalk(int x, int y)
        {
            if (x <= 0 || y <= 0 || x >= Width || y >= Height) return false;
            return true;
            //rest latter
        }
    }
}