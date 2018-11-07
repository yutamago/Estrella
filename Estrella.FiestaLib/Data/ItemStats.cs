using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.FiestaLib.Data
{
    public class ItemStats
    {
        public ushort Str { get; set; }
        public ushort End { get; set; }
        public ushort Dex { get; set; }
        public ushort Int { get; set; }
        public ushort Spr { get; set; }

        public static ItemStats LoadItemStatsFromDatabase(DataRow row)
        {
            var Stats = new ItemStats
            {
                Dex = GetDataTypes.GetUshort(row["Dex"]),
                End = GetDataTypes.GetUshort(row["con"]),
                Int = GetDataTypes.GetUshort(row["Int"]),
                Str = GetDataTypes.GetUshort(row["Str"])
            };
            return Stats;
        }
    }
}