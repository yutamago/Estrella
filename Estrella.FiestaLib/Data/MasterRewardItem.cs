using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.FiestaLib.Data
{
    public class MasterRewardItem : MasterRewardState
    {
        public MasterRewardItem()
        {
        }

        public MasterRewardItem(DataRow row)
        {
            ItemID = GetDataTypes.GetUshort(row["ItemID"]);
            Level = GetDataTypes.GetByte(row["Level"]);
            Job = (Job) GetDataTypes.GetByte(row["Job"]);
            Count = GetDataTypes.GetByte(row["Count"]);
        }

        public byte Level { get; private set; }
        public Job Job { get; private set; }
    }
}