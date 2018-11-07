using System;
using System.Collections.Generic;
using Estrella.FiestaLib.Data;

namespace Estrella.Zone.Game
{
    class RandomDrop
    {
        private Random ran = new Random();

        public RandomDrop(Mob mob)
        {
            dropcounter = 0;
            Monster = mob;
            GenerateDrop();
        }

        private Mob Monster { get; set; }
        private byte dropcounter { get; set; }

        void GenerateDrop()
        {
            foreach (var DropInfo in Monster.Info.Drops)
            {
                var rate = (float) (ran.NextDouble() * Monster.Info.Drops.Count);
                var RandomRate = rate * 100.0f / Monster.Info.Drops.Count;
                if (RandomRate < DropInfo.Rate)
                {
                    if (dropcounter >= DropInfo.Group.MaxCount) return;
                    DropItems(DropInfo.Group.Items, DropInfo.Rate, DropInfo.Group.MinCount, DropInfo.Group.MaxCount);
                }
                else
                {
                    dropcounter = 0;
                    return;
                }
            }
        }

        void DropItems(List<ItemInfo> Items, float Rate, byte Mincount, byte MaxCount)
        {
            foreach (var litem in Items)
            {
                if (dropcounter >= MaxCount) return;
                var index = (int) (ran.NextDouble() * Items.Count);
                var rate = (float) (ran.NextDouble() * Items.Count);
                var RandomRate = rate * 100.0f / Items.Count;
                if (RandomRate < Rate)
                {
                    if (litem.Type != ItemType.Equip)
                    {
                        var Amount = (ushort) new Random().Next(1, 255);
                        var DropItem = new Item(0, 0, Items[index].ItemID, 0, Amount);
                        DropItem.UpgradeStats = new UpgradeStats();
                        Monster.DropItem(DropItem);
                    }
                    else
                    {
                        var DropEq = new Item(0, 0, Items[index].ItemID, 0);
                        DropEq.UpgradeStats = new UpgradeStats();
                        Monster.DropItem(DropEq);
                    }

                    dropcounter++;
                }
                else
                {
                    dropcounter = 0;
                    return;
                }
            }
        }
    }
}