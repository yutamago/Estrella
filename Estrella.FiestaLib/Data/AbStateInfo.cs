using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.FiestaLib.Data
{
    public class AbStateInfo
    {
        public ushort ID { get; set; }
        public string InxName { get; set; }

        public static AbStateInfo LoadFromDatabase(DataRow row)
        {
            var info = new AbStateInfo
            {
                ID = GetDataTypes.GetUshort(row["ID"]),
                InxName = (string) row["InxName"]
            };
            return info;
        }
    }
}