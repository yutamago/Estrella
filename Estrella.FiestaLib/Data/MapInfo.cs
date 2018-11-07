using System.Collections.Generic;
using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.FiestaLib.Data
{
    public sealed class MapInfo
    {
        public MapInfo()
        {
        }

        public MapInfo(ushort id, string shortname, string fullname, int regenx, int regeny, byte kingdom,
            ushort viewrange)
        {
            ID = id;
            ShortName = shortname;
            FullName = fullname;
            RegenX = regenx;
            RegenY = regeny;
            Kingdom = kingdom;
            ViewRange = viewrange;
        }

        public ushort ID { get; private set; }
        public string ShortName { get; private set; }
        public string FullName { get; private set; }
        public int RegenX { get; private set; }
        public int RegenY { get; private set; }
        public byte Kingdom { get; private set; }
        public ushort ViewRange { get; private set; }

        public List<ShineNpc> NPCs { get; set; }

        public static MapInfo Load(DataRow row)
        {
            var info = new MapInfo
            {
                ID = GetDataTypes.GetUshort(row["ID"]),
                ShortName = (string) row["MapName"],
                FullName = (string) row["Name"],
                RegenX = GetDataTypes.GetInt(row["RegenX"]),
                RegenY = GetDataTypes.GetInt(row["RegenY"]),
                Kingdom = GetDataTypes.GetByte(row["KingdomMap"]),
                ViewRange = GetDataTypes.GetUshort(row["Sight"])
            };
            return info;
        }
    }
}