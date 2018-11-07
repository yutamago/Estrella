using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Estrella.World.Data
{
    public sealed class Inventory
    {
        private Mutex locker = new Mutex();

        public Inventory()
        {
            EquippedItems = new List<Equip>();
        }

        public List<Equip> EquippedItems { get; private set; }

        public void Enter()
        {
            locker.WaitOne();
        }

        public void Release()
        {
            locker.ReleaseMutex();
        }

        public void AddToEquipped(Equip pEquip)
        {
            try
            {
                locker.WaitOne();
                EquippedItems.Add(pEquip);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void LoadBasic(WorldCharacter pChar)
        {
            try
            {
                locker.WaitOne();
                DataTable equips = null;
                using (var dbClient = Program.DatabaseManager.GetClient())
                {
                    equips = dbClient.ReadDataTable("SELECT * FROM equips WHERE Owner=" + pChar.ID + " AND Slot < 0");
                }

                if (equips != null)
                {
                    foreach (DataRow row in equips.Rows)
                    {
                        var loaded = Equip.LoadEquip(row);
                        EquippedItems.Add(loaded);
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
    }
}