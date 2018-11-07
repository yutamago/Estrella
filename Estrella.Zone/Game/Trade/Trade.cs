using System.Collections.Generic;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Zone.Networking;

namespace Estrella.Zone.Game.Trade
{
    public sealed class Trade
    {
        public Trade(ZoneCharacter pFrom, ZoneCharacter pTo)
        {
            pCharFrom = pFrom;
            pCharTo = pTo;
            pCharFrom.Trade = this;
            pCharTo.Trade = this;
            SendTradeBeginn();
        }

        public ZoneCharacter pCharTo { get; private set; }
        public List<TradeItem> pToHandelItemList = new List<TradeItem>();

        private long pToHandelMoney { get; set; }
        private bool pToLocket { get; set; }
        private bool pToAgree { get; set; }
        public byte pToItemCounter { get; private set; }

        private long pFromHandelMoney { get; set; }
        private bool pFromLocket { get; set; }
        private bool pFromAgree { get; set; }

        public List<TradeItem> pFromHandelItemList = new List<TradeItem>();
        public ZoneCharacter pCharFrom { get; private set; }
        public byte pFromItemCounter { get; private set; }

        public void ChangeMoneyToTrade(ZoneCharacter pChar, long money)
        {
            if (pCharFrom == pChar)
            {
                pFromHandelMoney = money;
                SendChangeMoney(pCharTo.Client, money);
            }
            else if (pCharTo == pCharTo)
            {
                pToHandelMoney = money;
                SendChangeMoney(pCharFrom.Client, money);
            }
        }

        public void RemoveItemToHandel(ZoneCharacter pChar, byte pSlot)
        {
            if (pCharFrom == pChar)
            {
                var item = pFromHandelItemList.Find(d => d.TradeSlot == pSlot);

                pFromHandelItemList.Remove(item);
                SendItemRemovFromHandel(pCharTo.Client, pSlot);
                SendItemRemoveMe(pCharFrom.Client, pSlot);
                pFromItemCounter--;
            }
            else if (pCharTo == pCharTo)
            {
                var item = pToHandelItemList.Find(d => d.TradeSlot == pSlot);
                pToHandelItemList.Remove(item);
                SendItemRemovFromHandel(pCharFrom.Client, pSlot);
                SendItemRemoveMe(pCharTo.Client, pSlot);
                pToItemCounter--;
            }
        }

        public void AddItemToHandel(ZoneCharacter pChar, byte pSlot)
        {
            Item pItem;
            if (!pChar.Inventory.InventoryItems.TryGetValue(pSlot, out pItem))
                return;
            if (pCharFrom == pChar)
            {
                var Item = new TradeItem(pChar, pSlot, pFromItemCounter, pItem);
                pFromHandelItemList.Add(Item);
                SendTradeAddItemTo(pCharTo.Client, pItem, pFromItemCounter);

                SendTradeAddItemMe(pCharFrom.Client, pSlot, pFromItemCounter);
                pFromItemCounter++;
            }
            else if (pCharTo == pChar)
            {
                var Item = new TradeItem(pChar, pSlot, pToItemCounter, pItem);
                pFromHandelItemList.Add(Item);
                SendTradeAddItemTo(pCharFrom.Client, pItem, pToItemCounter);
                SendTradeAddItemMe(pCharTo.Client, pSlot, pToItemCounter);
                pToItemCounter++;
            }
        }

        public void TradeLock(ZoneCharacter pChar)
        {
            if (pCharFrom == pChar)
            {
                pFromLocket = true;
                if (pFromLocket && pToLocket)
                {
                    SendTradeLock(pCharFrom.Client);
                    SendTradeRdy();
                }
                else
                {
                    SendTradeLock(pCharTo.Client);
                }
            }
            else if (pCharTo == pCharTo)
            {
                pToLocket = true;
                if (pFromLocket && pToLocket)
                {
                    SendTradeLock(pCharFrom.Client);
                    SendTradeRdy();
                }
                else
                {
                    SendTradeLock(pCharTo.Client);
                }
            }
        }

        public void TradeBreak(ZoneCharacter pChar)
        {
            if (pCharFrom == pChar)
            {
                SendTradeBreak(pCharTo.Client);
                pCharTo.Trade = null;
            }
            else if (pCharTo == pCharTo)
            {
                SendTradeBreak(pCharFrom.Client);
                pCharFrom = null;
            }
        }

        public void AcceptTrade(ZoneCharacter pChar)
        {
            if (pCharTo == pChar)
            {
                pToAgree = true;
                SendTradeAgreeMe(pCharTo.Client);
                SendTradeAgreepTo(pCharFrom.Client);
            }
            else if (pCharFrom == pChar)
            {
                pFromAgree = true;
                SendTradeAgreeMe(pCharFrom.Client);
                SendTradeAgreepTo(pCharTo.Client);
            }

            if (pFromAgree && pToAgree && pFromLocket && pToLocket)
            {
                TradeComplett();
            }
        }

        private void SendPacketToAllTradeVendors(Packet packet)
        {
            pCharFrom.Client.SendPacket(packet);
            pCharTo.Client.SendPacket(packet);
        }

        private void TradeComplett()
        {
            foreach (var Item in pFromHandelItemList)
            {
                pCharFrom.Inventory.RemoveInventory(Item.Item);
                Item.Item.Owner = (uint) pCharTo.ID;
                sbyte pSlot;
                pCharTo.GetFreeInventorySlot(out pSlot);
                Item.Item.Slot = pSlot;
                pCharTo.GiveItem(Item.Item);
            }

            foreach (var Item in pToHandelItemList)
            {
                pCharTo.Inventory.RemoveInventory(Item.Item);
                Item.Item.Owner = (uint) pCharTo.ID;
                sbyte pSlot;
                pCharFrom.GetFreeInventorySlot(out pSlot);
                Item.Item.Slot = pSlot;
                pCharFrom.GiveItem(Item.Item);
            }

            SendTradeComplett();
            pCharFrom.Trade = null;
            pCharTo.Trade = null;
            var pToMoney = pCharTo.Character.Money + pFromHandelMoney - pToHandelMoney;
            var pFromMoney = pCharFrom.Character.Money + pToHandelMoney - pFromHandelMoney;
            pCharFrom.ChangeMoney(pFromMoney);
            pCharTo.ChangeMoney(pToMoney);
        }

        private void SendTradeLock(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeLock))
            {
                pClient.SendPacket(packet);
            }
        }

        private void SendTradeAddItemMe(ZoneClient pClient, byte pSlot, byte TradeSlot)
        {
            using (var packet = new Packet(SH19Type.SendAddItemSuccefull))
            {
                packet.WriteByte(pSlot);
                packet.WriteByte(TradeSlot);
                pClient.SendPacket(packet);
            }
        }

        private void SendTradeAddItemTo(ZoneClient pClient, Item pItem, byte TradepSlot)
        {
            using (var packet = new Packet(SH19Type.SendAddItem))
            {
                packet.WriteByte(TradepSlot);
                if (pItem.ItemInfo.Slot == ItemSlot.None)

                    pItem.WriteStats(packet);

                else

                    pItem.WriteEquipStats(packet);

                pClient.SendPacket(packet);
            }
        }

        private void SendTradeBeginn()
        {
            using (var packet = new Packet(SH19Type.SendTradeAccept))
            {
                packet.WriteUShort(pCharFrom.MapObjectID);
                pCharTo.Client.SendPacket(packet);
            }

            using (var packet = new Packet(SH19Type.SendTradeAccept))
            {
                packet.WriteUShort(pCharTo.MapObjectID);
                pCharFrom.Client.SendPacket(packet);
            }
        }

        private void SendTradeRdy()
        {
            using (var packet = new Packet(SH19Type.SendTradeRdy))
            {
                SendPacketToAllTradeVendors(packet);
            }
        }

        private void SendTradeAgreeMe(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeAgreeMe))
            {
                pClient.SendPacket(packet);
            }
        }

        private void SendTradeAgreepTo(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeAgreeTo))
            {
                pClient.SendPacket(packet);
            }
        }

        private void SendTradeComplett()
        {
            using (var packet = new Packet(SH19Type.SendTradeComplett))
            {
                SendPacketToAllTradeVendors(packet);
            }
        }

        private void SendChangeMoney(ZoneClient pClient, long money)
        {
            using (var packet = new Packet(SH19Type.SendChangeMoney))
            {
                packet.WriteLong(money);
                pClient.SendPacket(packet);
            }
        }

        private void SendTradeBreak(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeBreak))
            {
                pClient.SendPacket(packet);
            }
        }

        private void SendItemRemovFromHandel(ZoneClient pClient, byte Slot)
        {
            using (var packet = new Packet(SH19Type.SendRemoveItemFromHandel))
            {
                packet.WriteByte(Slot);
                pClient.SendPacket(packet);
            }
        }

        private void SendItemRemoveMe(ZoneClient pClient, byte pTradeSlot)
        {
            using (var packet = new Packet(SH19Type.SendItemRemove))
            {
                packet.WriteByte(pTradeSlot);
                pClient.SendPacket(packet);
            }
        }
    }
}