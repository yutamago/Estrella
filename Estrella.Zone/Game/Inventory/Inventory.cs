using System.Collections.Generic;
using System.Data;
using System.Threading;
using Estrella.FiestaLib;
using Estrella.Zone.Handlers;

namespace Estrella.Zone.Game.Inventory
{
    public class Inventory
    {
        private Mutex locker = new Mutex();

        public Inventory(ZoneCharacter pChar)
        {
            InventoryCount = 2;
            InventoryOwner = pChar;
            InventoryItems = new Dictionary<byte, Item>();
            EquippedItems = new List<Item>();
        }

        public Inventory()
        {
        }

        public long Money { get; set; }
        public List<Item> EquippedItems { get; private set; }
        public Dictionary<byte, Item> InventoryItems { get; private set; }
        public byte InventoryCount { get; private set; }
        private ZoneCharacter InventoryOwner { get; set; }

        public void Enter()
        {
            locker.WaitOne();
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

        public void LoadFull(ZoneCharacter pChar)
        {
            try

            {
                locker.WaitOne();
                DataTable items = null;
                using (var dbClient = Program.CharDBManager.GetClient())
                {
                    items = dbClient.ReadDataTable("SELECT * FROM items WHERE Owner=" + pChar.ID + "");
                }
                //we load all equippeditem

                if (items != null)
                {
                    foreach (DataRow row in items.Rows)
                    {
                        var loaded = Item.LoadItem(row);
                        loaded.Owner = (uint) pChar.ID;
                        loaded.UpgradeStats = new UpgradeStats();
                        if (loaded.IsEquipped)
                        {
                            loaded.Slot = (sbyte) loaded.ItemInfo.Slot;
                            EquippedItems.Add(loaded);
                        }
                        else
                        {
                            InventoryItems.Add((byte) loaded.Slot, loaded);
                        }
                    }
                }

                //we load inventory slots
                if (items != null)
                {
                    foreach (DataRow row in items.Rows)
                    {
                        var loaded = Item.LoadItem(row);
                        /*  if (loaded.ItemInfo.Class == ItemClass.Rider)
                          {
                              Mount mount = Data.DataProvider.Instance.GetMountByItemID(loaded.ID);
                              if (mount != null)
                              {
                                  loaded.Mount = mount;
                                  loaded.Mount.Food = GetDataTypes.GetUshort(row["fuelcount"]);
                                  loaded.Mount.ItemSlot = (byte)loaded.Slot;
                              }
                              this.AddToInventory(loaded);
                          }
                          else
                          {*/
                        AddToInventory(loaded);
                        //}
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public Item GetEquiptBySlot(byte slot, out Item Eq)
        {
            Eq = EquippedItems.Find(d => d.Slot == slot);
            return Eq;
        }

        public void RemoveInventory(Item pItem)
        {
            try
            {
                locker.WaitOne();
                Handler12.ModifyInventorySlot(InventoryOwner, 0x24, (byte) pItem.Slot, 0, null);
                pItem.Delete();
                InventoryItems.Remove((byte) pItem.Slot);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void AddToInventory(Item pItem)
        {
            try
            {
                locker.WaitOne();
                if (InventoryItems.ContainsKey((byte) pItem.Slot))
                {
                    InventoryItems[(byte) pItem.Slot].Delete(); //removes from DB
                    InventoryItems.Remove((byte) pItem.Slot);
                }

                InventoryItems.Add((byte) pItem.Slot, pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public void AddToEquipped(Item pEquip)
        {
            try
            {
                locker.WaitOne();
                var old = EquippedItems.Find(equip => equip.Slot == pEquip.Slot);
                if (old != null)
                {
                    old.IsEquipped = false;
                    AddToInventory(old);
                    EquippedItems.Remove(old);
                }

                EquippedItems.Add(pEquip);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public ushort GetEquippedBySlot(ItemSlot pSlot)
        {
            //double check if found
            var equip = EquippedItems.Find(d => d.Slot == (sbyte) pSlot && d.IsEquipped);
            if (equip == null)
            {
                return 0xffff;
            }

            return equip.ID;
        }

        public bool GetEmptySlot(out byte pSlot) //cpu intensive?
        {
            pSlot = 0;
            for (byte i = 0; i < InventoryCount * 24; ++i)
            {
                if (!InventoryItems.ContainsKey(i))
                {
                    pSlot = i;
                    return true;
                }
            }

            return false; //no more empty slots found
        }
    }
}