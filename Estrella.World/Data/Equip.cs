using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.World.Data
{
    public class Equip
    {
        public Equip(uint pOwner, ushort pEquipId, sbyte pSlot)
        {
            EquipId = pEquipId;
            Owner = pOwner;
            if (pSlot < 0)
            {
                Slot = pSlot;
                IsEquipped = true;
            }
            else
            {
                Slot = pSlot;
            }
        }

        public byte Upgrades { get; set; }
        public byte StatCount { get; private set; }
        public bool IsEquipped { get; set; }
        public ushort Str { get; private set; }
        public ushort End { get; private set; }
        public ushort Dex { get; private set; }
        public ushort Int { get; private set; }
        public ushort Spr { get; private set; }
        public sbyte Slot { get; set; }
        public ushort EquipId { get; set; }
        public ulong UniqueId { get; set; }
        public uint Owner { get; set; }

        public static Equip LoadEquip(DataRow row)
        {
            var uniqueId = GetDataTypes.GetUlong(row["ID"]);
            var owner = GetDataTypes.GetUint(row["Owner"]);
            var equipId = GetDataTypes.GetUshort(row["EquipID"]);
            var slot = GetDataTypes.GetSByte(row["Slot"]);
            var upgrade = GetDataTypes.GetByte(row["Upgrades"]);

            var strByte = GetDataTypes.GetUshort(row["iSTR"]);
            var endByte = GetDataTypes.GetUshort(row["iEND"]);
            var dexByte = GetDataTypes.GetUshort(row["iDEX"]);
            var sprByte = GetDataTypes.GetUshort(row["iSPR"]);
            var intByte = GetDataTypes.GetUshort(row["iINT"]);
            var equip = new Equip(owner, equipId, slot)
            {
                UniqueId = uniqueId,
                Upgrades = upgrade,
                Str = strByte,
                End = endByte,
                Dex = dexByte,
                Spr = sprByte,
                Int = intByte
            };
            return equip;
        }
    }
}