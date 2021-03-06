﻿using System.Data;
using Estrella.Database.DataStore;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Zone.Data;

namespace Estrella.Zone.Game
{
    public sealed class RewardItem : Item
    {
        public override ushort ID { get; set; }
        public override sbyte Slot { get; set; }
        public override UpgradeStats UpgradeStats { get; set; }
        public int CharID { get; set; }
        public ushort PageID { get; set; }

        public override ItemInfo ItemInfo
        {
            get { return DataProvider.Instance.GetItemInfo(ID); }
        }

        public void AddToDatabase()
        {
            Program.CharDBManager.GetClient()
                .ExecuteQuery("INSERT INTO  Rewarditems (CharID,Slot,ItemID,PageID) VALUES ('" + CharID + "','" +
                              Slot + "','" + ID + "','" + PageID + "')");
        }

        public void RemoveFromDatabase()
        {
            Program.CharDBManager.GetClient()
                .ExecuteQuery("DELETE FROM Rewarditems WHERE CharID='" + CharID + "' AND ItemID='" + ID +
                              "'");
        }

        public override void WriteInfo(Packet pPacket, bool WriteStats = true)
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

            byte lenght = 9; //later
            pPacket.WriteByte(lenght);
            pPacket.WriteByte((byte) Slot); //itemslot
            pPacket.WriteByte(0x08); //unk
            if (WriteStats)
            {
                if (ItemInfo.Slot == ItemSlot.None)
                    this.WriteStats(pPacket);
                else
                    WriteEquipStats(pPacket);
            }
        }

        public static RewardItem LoadFromDatabase(DataRow row)
        {
            var ppItem = new RewardItem
            {
                Slot = GetDataTypes.GetSByte(row["Slot"]),
                ID = GetDataTypes.GetUshort(row["ItemID"]),
                CharID = GetDataTypes.GetInt(row["CharID"]),
                PageID = GetDataTypes.GetByte(row["PageID"])
            };

            return ppItem;
        }
    }
}