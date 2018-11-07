using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.FiestaLib.Data
{
    public class MasterRewardState
    {
        public MasterRewardState()
        {
        }

        public MasterRewardState(DataRow row)
        {
            Str = GetDataTypes.GetUshort(row["Str"]);
            End = GetDataTypes.GetUshort(row["End"]);
            Dex = GetDataTypes.GetUshort(row["Dex"]);
            Int = GetDataTypes.GetUshort(row["Int"]);
            Spr = GetDataTypes.GetUshort(row["Spr"]);
            ItemID = GetDataTypes.GetUshort(row["ItemID"]);
        }

        public ushort ItemID { get; set; }
        public byte Upgrades { get; set; }
        public byte Count { get; set; }

        public ushort Str { get; private set; }
        public ushort End { get; private set; }
        public ushort Dex { get; private set; }
        public ushort Int { get; private set; }
        public ushort Spr { get; private set; }
    }
}