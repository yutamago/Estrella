using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.Database.Storage
{
    public class LookInfo
    {
        public byte Hair { get; set; }
        public byte HairColor { get; set; }
        public byte Face { get; set; }
        public bool Male { get; set; }

        public void ReadFromDatabase(DataRow row)
        {
            Male = ReadMethods.EnumToBool(row["Male"].ToString());
            Hair = byte.Parse(row["Hair"].ToString());
            HairColor = byte.Parse(row["HairColor"].ToString());
            Face = byte.Parse(row["Face"].ToString());
        }
    }
}