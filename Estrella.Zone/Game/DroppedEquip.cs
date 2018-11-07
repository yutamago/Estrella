namespace Estrella.Zone.Game
{
    public class DroppedEquip : DroppedItem
    {
        public DroppedEquip(Item pBase)
        {
            Amount = 1;
            //this.Expires = pBase.Expires;
            Dex = pBase.UpgradeStats.Dex;
            Str = pBase.UpgradeStats.Str;
            End = pBase.UpgradeStats.End;
            Int = pBase.UpgradeStats.Int;
            Upgrades = pBase.UpgradeStats.Upgrades;
            ItemID = pBase.ID;
        }

        public ushort Dex { get; set; }
        public ushort Str { get; set; }
        public ushort End { get; set; }
        public ushort Int { get; set; }
        public ushort Spr { get; set; }
        public ushort Upgrades { get; set; }
    }
}