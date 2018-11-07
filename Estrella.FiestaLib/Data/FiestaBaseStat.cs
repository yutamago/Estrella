using System;
using System.Data;

namespace Estrella.FiestaLib.Data
{
    public sealed class FiestaBaseStat
    {
        public Job Job { get; private set; }
        public int Level { get; private set; }
        public int Strength { get; private set; }
        public int Endurance { get; private set; }
        public int Intelligence { get; private set; }
        public int Dexterity { get; private set; }
        public int Spirit { get; private set; }
        public int SoulHP { get; private set; }
        public int MaxSoulHP { get; private set; }
        public int PriceHPStone { get; private set; }
        public int SoulSP { get; private set; }
        public int MaxSoulSP { get; private set; }
        public int PriceSPStone { get; private set; }
        public int AtkPerAP { get; private set; }
        public int DmgPerAP { get; private set; }
        public int MaxPwrStone { get; private set; }
        public int NumPwrStone { get; private set; }
        public int PricePwrStone { get; private set; }
        public int PwrStoneWC { get; private set; }
        public int PwrStoneMA { get; private set; }
        public int MaxGrdStone { get; private set; }
        public int NumGrdStone { get; private set; }
        public int PriceGrdStone { get; private set; }
        public int GrdStoneAC { get; private set; }
        public int GrdStoneMR { get; private set; }
        public int PainRes { get; private set; }
        public int RestraintRes { get; private set; }
        public int CurseRes { get; private set; }
        public int ShockRes { get; private set; }
        public uint MaxHP { get; private set; }
        public uint MaxSP { get; private set; }
        public int CharTitlePt { get; private set; }
        public int SkillPwrPt { get; private set; }
        public int HPStoneEffectID { get; private set; }
        public int SPStoneEffectID { get; private set; }

        public static FiestaBaseStat Load(DataRow row, Job job)
        {
            var info = new FiestaBaseStat
            {
                Job = job,
                Level = (int) row["Level"],
                Strength = (int) row["Strength"],
                Endurance = (int) row["Constitution"],
                Intelligence = (int) row["Intelligence"],
                Dexterity = (int) row["Dexterity"],
                Spirit = (int) row["MentalPower"],
                SoulHP = (int) row["SoulHP"],
                MaxSoulHP = (int) row["MAXSoulHP"],
                PriceHPStone = (int) row["PriceHPStone"],
                SoulSP = (int) row["SoulSP"],
                MaxSoulSP = (int) row["MAXSoulSP"],
                PriceSPStone = (int) row["PriceSPStone"],
                AtkPerAP = (int) row["AtkPerAP"],
                DmgPerAP = (int) row["DmgPerAP"],
                MaxPwrStone = (int) row["MaxPwrStone"],
                NumPwrStone = (int) row["NumPwrStone"],
                PricePwrStone = (int) row["PricePwrStone"],
                PwrStoneWC = (int) row["PwrStoneWC"],
                PwrStoneMA = (int) row["PwrStoneMA"],
                MaxGrdStone = (int) row["MaxGrdStone"],
                NumGrdStone = (int) row["NumGrdStone"],
                PriceGrdStone = (int) row["PriceGrdStone"],
                GrdStoneAC = (int) row["GrdStoneAC"],
                GrdStoneMR = (int) row["GrdStoneMR"],
                PainRes = (int) row["PainRes"],
                RestraintRes = (int) row["RestraintRes"],
                CurseRes = (int) row["CurseRes"],
                ShockRes = (int) row["ShockRes"],
                MaxHP = (ushort) (int) row["MaxHP"],
                MaxSP = (ushort) (int) row["MaxSP"],
                CharTitlePt = (int) row["CharTitlePt"],
                SkillPwrPt = (int) row["SkillPwrPt"],
                SPStoneEffectID = (int) row["SPStoneEffectID"],
                HPStoneEffectID = (int) row["HPStoneEffectID"]
            };

            return info;
        }
    }
}