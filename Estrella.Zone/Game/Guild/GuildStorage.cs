using System;
using System.Collections.Generic;
using System.Data;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Zone.Data;

namespace Estrella.Zone.Game.Guild
{
    public sealed class GuildStorage
    {
        public GuildStorage(Guild G)
        {
            GuildStorageItems = new Dictionary<byte, Item>();
            Guild = G;
            LoadGuildStorageFromDatabase(G.ID);
        }

        public Dictionary<byte, Item> GuildStorageItems { get; private set; }
        public Guild Guild { get; private set; }

        public void SendAddGuildStore(GuildStoreAddFlags Flags, string Charname, long Value, long NewGuildMoney = 0,
            ushort ItemID = 0xFFFF)
        {
            using (var packet = new Packet(SH38Type.AddToGuildStore))
            {
                packet.WriteByte(0); //unk
                packet.WriteByte((byte) Flags);
                packet.WriteString(Charname, 16);
                packet.WriteUShort(ItemID);
                packet.WriteByte(0);
                packet.WriteLong(Value);
                packet.WriteLong(NewGuildMoney); //new GuildMoney
                Guild.Broadcast(packet);
            }
        }

        public void SendRemoveFromGuildStore(GuildStoreAddFlags Flags, string Charname, long Value,
            long NewGuildMoney = 0, ushort ItemID = 0xFFFF)
        {
            using (var packet = new Packet(SH38Type.RemoveFromGuildStore))
            {
                packet.WriteByte(0); //unk
                packet.WriteByte((byte) Flags);
                packet.WriteString(Charname, 16);
                packet.WriteUShort(ItemID);
                packet.WriteByte(0);
                packet.WriteLong(Value);
                packet.WriteLong(NewGuildMoney); //new GuildMoney
                Guild.Broadcast(packet);
            }
        }

        public void SaveStoreItem(int GuildID, ushort ItemID, byte pSlot)
        {
        }

        public void RemoveStoreItem(int GuildID, ushort ItemID)
        {
        }

        public bool GetHasFreeGuildStoreSlot()
        {
            for (byte i = 0; i < 92; i++)
            {
                if (!GuildStorageItems.ContainsKey(i))
                {
                    return true;
                }
            }

            return false;
        }

        private void LoadGuildStorageFromDatabase(int GuildID)
        {
            DataTable GuildItemData = null;
            using (var DBClient = Program.CharDBManager.GetClient())
            {
                GuildItemData = DBClient.ReadDataTable("SELECT * FROM GuildStorage WHERE GuildID=" + GuildID + "");
            }

            if (GuildItemData != null)
            {
                foreach (DataRow row in GuildItemData.Rows)
                {
                    var ItemID = Convert.ToUInt16(row["ItemID"]);
                    var Amount = Convert.ToUInt16(row["Amount"]);
                    var pSlot = Convert.ToByte(row["Slot"]);
                    var pItem = new Item(GuildID, ItemID, pSlot, Amount);
                    GuildStorageItems.Add(pSlot, pItem);
                }
            }
        }
    }
}