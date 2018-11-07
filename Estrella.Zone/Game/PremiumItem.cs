using System.Data;
using Estrella.Database.DataStore;
using Estrella.FiestaLib.Networking;

namespace Estrella.Zone.Game
{
    public class PremiumItem
    {
        public int UniqueID { get; set; }
        public int ShopID { get; set; }
        public int CharID { get; set; }
        public byte PageID { get; set; }
        public byte Slot { get; set; }

        public void WritePremiumInfo(Packet packet)
        {
            packet.WriteInt(UniqueID);
            packet.WriteInt(ShopID);
            packet.WriteInt(0); //unk
            packet.WriteInt(0); //unk
        }

        public virtual void RemoveFromDatabase()
        {
            Program.CharDBManager.GetClient().ExecuteQuery("DELETE FROM PremiumItem WHERE CharID='" + CharID +
                                                           "' AND UniqueID='" + UniqueID + "'");
        }

        public virtual void AddToDatabase()
        {
            Program.CharDBManager.GetClient()
                .ExecuteQuery("INSERT INTO PremiumItems (CharID,ShopID,UniqueID,PageID) VALUES ('" + CharID +
                              "','" + ShopID + "','" + UniqueID + "','" + PageID + "')");
        }

        public static PremiumItem LoadFromDatabase(DataRow row)
        {
            var ppItem = new PremiumItem
            {
                UniqueID = GetDataTypes.GetInt(row["UniqueID"]),
                Slot = GetDataTypes.GetByte(row["PageID"]),
                ShopID = GetDataTypes.GetInt(row["ShopID"]),
                CharID = GetDataTypes.GetInt(row["CharID"]),
                PageID = GetDataTypes.GetByte(row["PageID"])
            };
            return ppItem;
        }
    }
}