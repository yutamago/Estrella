/* Thanks no0dl for the base Function And Packet Structures in this File copright 2012*/

using System;
using System.Data;
using Estrella.Database.DataStore;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Zone.Data;
using MySql.Data.MySqlClient;

namespace Estrella.Zone.Game
{
    public class Item
    {
        private const string GiveItem = "give_item;";
        private const string UpdateItem = "update_item;";
        private const string DeleteItem = "DELETE FROM items WHERE ID=@id";

        public Item(ulong UniqueID, uint pOwner, ushort pID, sbyte Slot, ushort Amount = 1)
        {
            ItemSlot type;
            if (!DataProvider.GetItemType(pID, out type))
            {
                throw new InvalidOperationException("Invalid item ID.");
            }

            this.Slot = (sbyte) type;
            Flags = ItemFlags.Normal;
            Owner = pOwner;
            ID = pID;
            this.Slot = Slot;
        }

        public Item(int GuildID, ushort pID, byte Slot, ushort Amount = 1)
        {
            ItemSlot type;
            if (!DataProvider.GetItemType(pID, out type))
            {
                throw new InvalidOperationException("Invalid item ID.");
            }

            this.Slot = (sbyte) type;
            Flags = ItemFlags.GuildItem;
            ID = pID;
            this.Slot = (sbyte) Slot;
            Ammount = Amount;
        }

        public Item()
        {
        }


        public virtual ushort ID { get; set; }

        public virtual ItemInfo ItemInfo
        {
            get { return DataProvider.Instance.GetItemInfo(ID); }
        }

        public virtual UpgradeStats UpgradeStats { get; set; }
        public bool IsEquipped { get; set; }
        public virtual sbyte Slot { get; set; }
        public virtual ushort Ammount { get; set; }
        public virtual uint Owner { get; set; }
        public virtual ulong UniqueID { get; set; }
        public ItemFlags Flags { get; set; }

        public bool Delete()
        {
            if (UniqueID > 0)
            {
                Program.DatabaseManager.GetClient()
                    .ExecuteQuery("DELETE FROM items WHERE ID=" + UniqueID + " AND Slot='" + Slot + "'");
                UniqueID = 0;
                Owner = 0;
                return true;
            }

            return false;
        }

        public void Save()
        {
            if (UniqueID == 0)
            {
                using (var command = new MySqlCommand(GiveItem))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@puniqueid", MySqlDbType.Int64);
                    command.Parameters["@puniqueid"].Direction = ParameterDirection.Output;
                    command.Parameters.AddWithValue("@powner", Owner);
                    command.Parameters.AddWithValue("@pslot", Slot);
                    command.Parameters.AddWithValue("@pitemid", ID);
                    command.Parameters.AddWithValue("@pamount", Ammount);
                    Program.CharDBManager.GetClient().ExecuteQueryWithParameters(command);
                    UniqueID = Convert.ToUInt64(command.Parameters["@puniqueid"].Value);
                }
            }
            else
            {
                using (var command = new MySqlCommand(UpdateItem))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@puniqueid", UniqueID);
                    command.Parameters.AddWithValue("@powner", Owner);
                    command.Parameters.AddWithValue("@pEquipt", IsEquipped);
                    command.Parameters.AddWithValue("@pslot", Slot);
                    command.Parameters.AddWithValue("@pamount", Ammount);
                    Program.CharDBManager.GetClient().ExecuteQueryWithParameters(command);
                }
            }
        }

        public static Item LoadItem(DataRow Row)
        {
            var id = GetDataTypes.GetUlong(Row["ID"]);
            var owner = GetDataTypes.GetUint(Row["Owner"]);
            var slot = GetDataTypes.GetSByte(Row["Slot"]);
            var equipID = GetDataTypes.GetUshort(Row["ItemID"]);

            var amount = GetDataTypes.GetUshort(Row["Amount"]);
            var item = new Item(id, owner, equipID, slot, amount)
            {
                Slot = slot,
                IsEquipped = GetDataTypes.GetBool(Row["Equipt"])
            };
            return item;
        }

        public virtual void WriteInfo(Packet Packet, bool WriteStats = true)
        {
            byte length;
            byte statCount;

            if (ItemInfo.Slot == ItemSlot.None)
            {
                length = GetInfoLength(ItemInfo.Class);
                statCount = 0;
            }
            else
            {
                length = GetEquipLength(this);
                statCount = GetInfoStatCount(this);
            }

            length += (byte) (statCount * 3);


            Packet.WriteByte(length);
            Packet.WriteByte((byte) Slot);
            if (Flags == ItemFlags.Normal)
            {
                Packet.WriteByte((byte) (IsEquipped ? 0x20 : 0x24));
            }
            else if (Flags == ItemFlags.GuildItem)
            {
                Packet.WriteByte(0); // 1 not display 0 display in store
            }

            if (WriteStats)
            {
                if (ItemInfo.Slot == ItemSlot.None)
                    this.WriteStats(Packet);
                else
                    WriteEquipStats(Packet);
            }
        }

        public void WriteStats(Packet Packet)
        {
            Packet.WriteUShort(ItemInfo.ItemID);


            switch (ItemInfo.Class)
            {
                case ItemClass.Mount:
                    Packet.WriteUShort(100); // food ?
                    Packet.WriteUInt(0); // use time?
                    Packet.WriteUInt(1992027391); // expire time?
                    Packet.WriteUShort(0); // unk ?
                    break;
                case ItemClass.QuestItem:
                    Packet.WriteByte(0); // unk ?
                    break;
                case ItemClass.PremiumItem:
                    Packet.WriteUInt(0); // use time
                    Packet.WriteUInt(1992027391); // expire time
                    break;

                case ItemClass.CollectCard:
                    Packet.WriteUInt(1000); // serial
                    Packet.WriteByte(5); // stars
                    break;
            }
        }

        public void WriteEquipStats(Packet Packet)
        {
            var length = GetEquipLength(this);
            var statCount = GetInfoStatCount(this);

            length += (byte) (statCount * 3);


            Packet.WriteUShort(ItemInfo.ItemID);


            switch (ItemInfo.Slot)
            {
                case ItemSlot.Weapon:
                case ItemSlot.Weapon2:
                case ItemSlot.Armor:
                case ItemSlot.Pants:
                case ItemSlot.Boots:
                case ItemSlot.Helm:
                case ItemSlot.Pet:
                    Packet.WriteByte(0); //upgrades
                    Packet.WriteByte(0); // unk
                    Packet.WriteUInt(0); // unk2
                    Packet.WriteByte(0); // unk3

                    if ((ItemInfo.Slot == ItemSlot.Weapon
                         || ItemInfo.Slot == ItemSlot.Weapon2)
                        && ItemInfo.Class != ItemClass.Shield)
                    {
                        Packet.WriteUShort(0xFFFF); // title mob 1
                        Packet.WriteUInt(0); // title mob 1 kill count
                        Packet.WriteUShort(0xFFFF); // title mob 2
                        Packet.WriteUInt(0); // title mob 2 kill count
                        Packet.WriteUShort(0xFFFF); // title mob 3
                        Packet.WriteUInt(0); // title mob 3 kill count

                        Packet.WriteUShort(0xFFFF); // unk
                        Packet.WriteString("", 16); // license adder name
                        Packet.WriteByte(0);
                    }

                    Packet.WriteUInt(1992027391); // unk4
                    break;


                case ItemSlot.Necklace:
                case ItemSlot.Earrings:
                case ItemSlot.Ring:
                case ItemSlot.Ring2:
                    Packet.WriteUInt(1992027391); // expire time?
                    Packet.WriteUInt(0); // time ?
                    Packet.WriteByte(0); //upgrades

                    Packet.WriteUShort(0); // refinement stats?
                    Packet.WriteUShort(UpgradeStats.Str);
                    Packet.WriteUShort(UpgradeStats.End);
                    Packet.WriteUShort(UpgradeStats.Dex);
                    Packet.WriteUShort(UpgradeStats.Int);
                    Packet.WriteUShort(UpgradeStats.Spr);

                    break;


                default:
                    Packet.WriteUInt(0);
                    Packet.WriteUInt(1992027391); // expire time?
                    break;
            }


            switch (ItemInfo.Slot)
            {
                case ItemSlot.Weapon:
                case ItemSlot.Weapon2:
                case ItemSlot.Armor:
                case ItemSlot.Pants:
                case ItemSlot.Boots:
                case ItemSlot.Helm:
                case ItemSlot.Pet:
                case ItemSlot.Necklace:
                case ItemSlot.Earrings:
                case ItemSlot.Ring:
                case ItemSlot.Ring2:
                    Packet.WriteByte((byte) (statCount << 1 | 1));


                    if (ItemInfo.Stats.Str > 0)
                    {
                        Packet.WriteByte(0);
                        Packet.WriteUShort(ItemInfo.Stats.Str);
                    }

                    if (ItemInfo.Stats.End > 0)
                    {
                        Packet.WriteByte(1);
                        Packet.WriteUShort(ItemInfo.Stats.End);
                    }

                    if (ItemInfo.Stats.Dex > 0)
                    {
                        Packet.WriteByte(2);
                        Packet.WriteUShort(ItemInfo.Stats.Dex);
                    }

                    if (ItemInfo.Stats.Spr > 0)
                    {
                        Packet.WriteByte(3);
                        Packet.WriteUShort(ItemInfo.Stats.Spr);
                    }

                    if (ItemInfo.Stats.Int > 0)
                    {
                        Packet.WriteByte(4);
                        Packet.WriteUShort(ItemInfo.Stats.Int);
                    }

                    break;
            }
        }

        public static byte GetInfoLength(ItemClass Class)
        {
            switch (Class)
            {
                case ItemClass.Mount:
                    return 16;

                case ItemClass.QuestItem:
                    return 6;

                case ItemClass.PremiumItem:
                    return 12;

                case ItemClass.CollectCard:
                    return 9;

                case ItemClass.Lizenz:
                case ItemClass.Scroll:
                case ItemClass.NonUse:
                default:
                    return 5;
            }
        }

        public static byte GetEquipLength(Item Item)
        {
            switch (Item.ItemInfo.Slot)
            {
                case ItemSlot.Weapon:
                case ItemSlot.Weapon2:
                    if (Item.ItemInfo.Class == ItemClass.Shield)
                        return 16;
                    else
                        return 53;


                case ItemSlot.Armor:
                case ItemSlot.Pants:
                case ItemSlot.Boots:
                case ItemSlot.Helm:
                case ItemSlot.Pet:
                    return 16;


                case ItemSlot.Necklace:
                case ItemSlot.Earrings:
                case ItemSlot.Ring:
                case ItemSlot.Ring2:
                    return 26;


                case ItemSlot.CostumeArmor:
                case ItemSlot.CostumeBoots:
                case ItemSlot.CostumeHelm:
                case ItemSlot.CostumePants:
                default:
                    return 12;
            }
        }

        public static byte GetInfoStatCount(Item Item)
        {
            byte count = 0;

            if (Item.ItemInfo.Stats.Str > 0)
                count++;

            if (Item.ItemInfo.Stats.End > 0)
                count++;

            if (Item.ItemInfo.Stats.Dex > 0)
                count++;

            if (Item.ItemInfo.Stats.Int > 0)
                count++;

            if (Item.ItemInfo.Stats.Spr > 0)
                count++;


            return count;
        }
    }
}