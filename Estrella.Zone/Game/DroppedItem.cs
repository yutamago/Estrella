﻿using System;
using Estrella.FiestaLib.Data;
using Estrella.Zone.Data;

namespace Estrella.Zone.Game
{
    public class DroppedItem
    {
        public DroppedItem()
        {
        }

        public DroppedItem(Item pBase)
        {
            Amount = pBase.Ammount;
            ItemID = pBase.ID;
            // Expires = pBase;
        }

        public int Amount { get; set; }
        public ushort ItemID { get; protected set; }
        public virtual DateTime? Expires { get; set; }

        public ItemInfo Info
        {
            get { return DataProvider.Instance.GetItemInfo(ItemID); }
        }
    }
}