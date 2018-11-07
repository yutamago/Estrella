using System;

namespace Estrella.Zone.Game.Trade
{
    public class TradeReqest
    {
        public TradeReqest(ZoneCharacter pFrom, ushort ToMapObjectID)
        {
            if (pFrom.SelectedObject.MapObjectID == ToMapObjectID)
            {
                CrationTimeStamp = DateTime.Now;
                pToTradeClient = pFrom.SelectedObject as ZoneCharacter;
                pFromTradeClient = pFrom;
                MapID = pFrom.MapID;
            }
        }

        public DateTime CrationTimeStamp { get; private set; }
        public ZoneCharacter pToTradeClient { get; private set; }
        public ZoneCharacter pFromTradeClient { get; private set; }
        public ushort MapID { get; private set; }
    }
}