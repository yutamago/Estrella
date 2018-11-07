using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Estrella.Zone.Game.Inventory
{
    public class PremiumInventory
    {
        private Mutex locker = new Mutex();

        public PremiumInventory()
        {
            PremiumItems = new Dictionary<ushort, List<PremiumItem>>();
            MaxPageCount = 1;
            for (byte i = 0; i < MaxPageCount; ++i)
            {
                PremiumItems[i] = new List<PremiumItem>();
            }
        }

        public Dictionary<ushort, List<PremiumItem>> PremiumItems { get; private set; }
        public ushort Count { get; private set; }
        private ushort MaxPageCount { get; set; }

        public void LoadPremiumItems(int pChar)
        {
            try
            {
                locker.WaitOne();
                DataTable Premiumdata = null;
                using (var dbClient = Program.CharDBManager.GetClient())
                {
                    Premiumdata = dbClient.ReadDataTable("SELECT *FROM PremiumItem WHERE CharID='" + pChar + "'");
                }

                if (Premiumdata != null)
                {
                    foreach (DataRow row in Premiumdata.Rows)
                    {
                        var pItem = PremiumItem.LoadFromDatabase(row);
                        PremiumItems[pItem.PageID].Add(pItem);
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void RemovePremiumItem(PremiumItem pItem)
        {
            try
            {
                locker.WaitOne();
                PremiumItems[pItem.PageID].Remove(pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void AddPremiumItem(PremiumItem pItem)
        {
            pItem.AddToDatabase();
            PremiumItems[pItem.PageID].Add(pItem);
        }

        public void Enter()
        {
            locker.WaitOne();
        }

        public bool GetEmptySlot(out byte pSlot, out ushort PageID) //cpu intensive?
        {
            pSlot = 0;
            PageID = 0;
            for (byte i = 0; i < Count; ++i)
            {
                if (!PremiumItems.ContainsKey(i))
                {
                    for (byte i2 = 0; i2 < PremiumItems[i].Count * 24; ++i2)
                    {
                        var Item = PremiumItems[i].Find(ss => ss.Slot == i2);
                        if (Item == null)
                        {
                            pSlot = i2;
                            PageID = i;
                            return true;
                        }
                    }
                }
            }

            return false; //no more empty slots found
        }

        public void Release()
        {
            try
            {
                locker.ReleaseMutex();
            }
            catch
            {
            }
        }
    }
}