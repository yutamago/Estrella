using System.Data;

namespace Estrella.Database.Storage
{
    public class PositionInfo
    {
        public int XPos { get; set; }
        public int YPos { get; set; }
        public ushort Map { get; set; }

        public void ReadFromDatabase(DataRow row)
        {
            Map = (ushort) row["Map"];
            XPos = int.Parse(row["XPos"].ToString());
            YPos = int.Parse(row["YPos"].ToString());
        }
    }
}