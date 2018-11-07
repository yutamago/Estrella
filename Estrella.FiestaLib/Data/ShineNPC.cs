using System;
using System.Collections.Generic;
using System.Data;

namespace Estrella.FiestaLib.Data
{
    public sealed class ShineNpc
    {
        public short MobID { get; private set; }
        public string MobName { get; private set; }
        public string Map { get; private set; }
        public int CoordX { get; private set; }
        public int CoordY { get; private set; }
        public short Direct { get; private set; }
        public byte NpcMenu { get; private set; }
        public string Role { get; private set; }
        public string RoleArg0 { get; private set; }
        public ushort Flags { get; private set; }
        public List<Vendor> VendorItems { get; set; }

        public static ShineNpc Load(DataRow row)
        {
            var info = new ShineNpc
            {
                MobID = (short) (int) row["MobID"],
                Flags = (ushort) row["Flags"],
                MobName = (string) row["MobName"],
                Map = (string) row["Map"],
                CoordX = (int) row["RegenX"],
                CoordY = (int) row["RegenY"],
                Direct = (short) (int) row["Direct"],
                NpcMenu = (byte) (sbyte) row["NPCMenu"],
                Role = (string) row["Role"],
                RoleArg0 = (string) row["RoleArg0"]
            };
            if (info.Flags == 1)
            {
                info.VendorItems = new List<Vendor>();
            }

            return info;
        }
    }
}