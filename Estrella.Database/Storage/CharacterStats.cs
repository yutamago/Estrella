using System.Data;

namespace Estrella.Database.Storage
{
    public class CharacterStats
    {
        public byte StrStats { get; set; }
        public byte EndStats { get; set; }
        public byte DexStats { get; set; }
        public byte IntStats { get; set; }
        public byte SprStats { get; set; }

        public ushort MinDamage { get; set; }
        public ushort MaxDamage { get; set; }
        public ushort MinMagic { get; set; }
        public ushort MaxMagic { get; set; }
        public ushort WeaponDef { get; set; }
        public ushort MagicDef { get; set; }

        public byte StrBonus { get; set; }
        public byte EndBonus { get; set; }
        public byte DexBonus { get; set; }
        public byte IntBonus { get; set; }
        public byte SprBonus { get; set; }

        public void ReadFromDatabase(DataRow row)
        {
            StrStats = byte.Parse(row["Str"].ToString());
            EndStats = byte.Parse(row["End"].ToString());
            DexStats = byte.Parse(row["Dex"].ToString());
            SprStats = byte.Parse(row["Spr"].ToString());
            IntStats = byte.Parse(row["StrInt"].ToString());
        }
    }
}