using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Zone.Data;

namespace Estrella.Zone.Game
{
    public class House
    {
        public enum HouseType
        {
            Resting,
            SellingVendor,
            BuyingVendor
        }

        public House(ZoneCharacter pOwner, HouseType pType, ushort pItemID = (ushort) 0, string pName = "")
        {
            Owner = pOwner;
            Type = pType;
            ItemID = pItemID;
            Name = pName;
        }

        public ushort ItemID { get; private set; }
        public string Name { get; private set; }
        public HouseType Type { get; private set; }
        public ZoneCharacter Owner { get; private set; }

        public MiniHouseInfo Info
        {
            get
            {
                return DataProvider.Instance.MiniHouses.ContainsKey(ItemID)
                    ? DataProvider.Instance.MiniHouses[ItemID]
                    : null;
            }
        }

        public void WritePacket(Packet pPacket)
        {
            pPacket.WriteUShort(ItemID);
            if (Type != HouseType.Resting)
            {
                pPacket.Fill(10, 0xFF); // Unknown

                pPacket.WriteString(Name, 30);
            }
            else
            {
                pPacket.WriteHexAsBytes("BE 02 FA 01 F8 01");
                pPacket.Fill(34, 0xFF); // No idea!?
            }

            pPacket.WriteByte(0xFF);
            pPacket.WriteByte(0); //unk
        }
    }
}