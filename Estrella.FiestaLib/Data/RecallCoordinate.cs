using System;
using System.Data;

namespace Estrella.FiestaLib.Data
{
    public sealed class RecallCoordinate
    {
        public string ItemIndex { get; private set; }
        public string MapName { get; private set; }
        public short LinkX { get; private set; }
        public short LinkY { get; private set; }

        public static RecallCoordinate Load(DataRow row)
        {
            var info = new RecallCoordinate
            {
                ItemIndex = row["ItemIndex"].ToString(),
                MapName = row["MapName"].ToString(),
                LinkX = short.Parse(row["LinkX"].ToString()),
                LinkY = short.Parse(row["LinkY"].ToString())
            };
            return info;
        }
    }
}