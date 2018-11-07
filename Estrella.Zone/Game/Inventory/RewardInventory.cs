using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Estrella.Zone.Game.Inventory
{
    public class RewardInventory
    {
        private Mutex locker = new Mutex();

        public RewardInventory()
        {
            RewardItems = new Dictionary<ushort, List<RewardItem>>();
            MaxPageCount = 1;
            for (byte i = 0; i < MaxPageCount; ++i)
            {
                RewardItems[i] = new List<RewardItem>();
            }
        }

        public Dictionary<ushort, List<RewardItem>> RewardItems { get; set; }
        private ushort MaxPageCount { get; set; }

        public void LoadRewardItems(int pCharID)
        {
            try
            {
                locker.WaitOne();
                DataTable Rewarddata = null;
                using (var dbClient = Program.CharDBManager.GetClient())
                {
                    Rewarddata = dbClient.ReadDataTable("SELECT *FROM RewardItems WHERE CharID='" + pCharID + "'");
                }

                if (Rewarddata != null)
                {
                    foreach (DataRow row in Rewarddata.Rows)
                    {
                        var pItem = RewardItem.LoadFromDatabase(row);
                        if (!RewardItems.ContainsKey(pItem.PageID))
                        {
                            RewardItems[pItem.PageID] = new List<RewardItem>();
                        }

                        RewardItems[pItem.PageID].Add(pItem);
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void RemoveRewardItem(RewardItem pItem)
        {
            try
            {
                locker.WaitOne();
                pItem.RemoveFromDatabase();
                RewardItems[pItem.PageID].Remove(pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void AddRewardItem(RewardItem pItem)
        {
            try
            {
                locker.WaitOne();
                if (!RewardItems.ContainsKey(pItem.PageID))
                {
                    RewardItems[pItem.PageID] = new List<RewardItem>();
                }

                pItem.AddToDatabase();
                RewardItems[pItem.PageID].Add(pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void Enter()
        {
            locker.WaitOne();
        }

        public bool GetEmptySlot(out byte pSlot, out ushort PageID) //cpu intensive?
        {
            pSlot = 0;
            PageID = 0;
            for (byte i = 0; i < RewardItems.Count; ++i)
            {
                for (byte i2 = 0; i2 < 24; ++i2)
                {
                    var Item = RewardItems[i].Find(ss => ss.Slot == i2);
                    if (Item == null)
                    {
                        pSlot = i2;
                        PageID = i;
                        return true;
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