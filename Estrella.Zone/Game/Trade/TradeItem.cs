namespace Estrella.Zone.Game.Trade
{
    public class TradeItem
    {
        public TradeItem(ZoneCharacter owner, byte InventorySlot, byte Tradeslot, Item pItem)
        {
            Owner = owner;
            Item = pItem;
            this.InventorySlot = InventorySlot;
            TradeSlot = Tradeslot;
        }

        public ZoneCharacter Owner { get; set; }
        public byte InventorySlot { get; set; }
        public byte TradeSlot { get; set; }
        public Item Item { get; set; }
    }
}