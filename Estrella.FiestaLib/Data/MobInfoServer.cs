using System;
using System.Data;
using Estrella.Database.DataStore;

namespace Estrella.FiestaLib.Data
{
    public sealed class MobInfoServer
    {
        public uint ID { get; private set; }
        public string InxName { get; private set; }
        public byte Visible { get; private set; }
        public ushort AC { get; private set; }
        public ushort TB { get; private set; }
        public ushort MR { get; private set; }
        public ushort MB { get; private set; }
        public uint EnemyDetectType { get; private set; }
        public uint MobKillInx { get; private set; }
        public uint MonExp { get; private set; }
        public ushort ExpRange { get; private set; }
        public ushort DetectCha { get; private set; }
        public byte ResetInterval { get; private set; }
        public ushort CutInterval { get; private set; }
        public uint CutNonAT { get; private set; }
        public uint FollowCha { get; private set; }
        public ushort PceHPRcvDly { get; private set; }
        public ushort PceHPRcv { get; private set; }
        public ushort AtkHPRcvDly { get; private set; }
        public ushort AtkHPRcv { get; private set; }
        public ushort Str { get; private set; }
        public ushort Dex { get; private set; }
        public ushort Con { get; private set; }
        public ushort Int { get; private set; }
        public ushort Men { get; private set; }
        public uint MobRaceType { get; private set; }
        public byte Rank { get; private set; }
        public uint FamilyArea { get; private set; }
        public uint FamilyRescArea { get; private set; }
        public byte FamilyRescCount { get; private set; }
        public ushort BloodingResi { get; private set; }
        public ushort StunResi { get; private set; }
        public ushort MoveSpeedResi { get; private set; }
        public ushort FearResi { get; private set; }
        public string ResIndex { get; private set; }
        public ushort KQKillPoint { get; private set; }
        public byte Return2Regen { get; private set; }
        public byte IsRoaming { get; private set; }
        public byte RoamingNumber { get; private set; }
        public ushort RoamingDistance { get; private set; }
        public ushort MaxSP { get; private set; }
        public byte BroadAtDead { get; private set; }
        public ushort TurnSpeed { get; private set; }
        public ushort WalkChase { get; private set; }
        public byte AllCanLoot { get; private set; }
        public ushort DmgByHealMin { get; private set; }
        public ushort DmgByHealMax { get; private set; }

        public static MobInfoServer Load(DataRow row)
        {
            var info = new MobInfoServer
            {
                ID = GetDataTypes.GetUint(row["ID"]),
                InxName = (string) row["InxName"],
                Visible = GetDataTypes.GetByte(row["Visible"]),
                AC = GetDataTypes.GetUshort(row["AC"]),
                TB = GetDataTypes.GetUshort(row["TB"]),
                MR = GetDataTypes.GetUshort(row["MR"]),
                MB = GetDataTypes.GetUshort(row["MB"]),
                EnemyDetectType = GetDataTypes.GetUint(row["EnemyDetectType"]),
                MobKillInx = GetDataTypes.GetUint(row["MobKillInx"]),
                MonExp = GetDataTypes.GetUint(row["MonEXP"]),
                ExpRange = GetDataTypes.GetUshort(row["EXPRange"]),
                DetectCha = GetDataTypes.GetUshort(row["DetectCha"]),
                ResetInterval = GetDataTypes.GetByte(row["ResetInterval"]),
                CutInterval = GetDataTypes.GetUshort(row["CutInterval"]),
                CutNonAT = GetDataTypes.GetUint(row["CutNonAT"]),
                FollowCha = GetDataTypes.GetUint(row["FollowCha"]),
                PceHPRcvDly = GetDataTypes.GetUshort(row["PceHPRcvDly"]),
                PceHPRcv = GetDataTypes.GetUshort(row["PceHPRcv"]),
                AtkHPRcvDly = GetDataTypes.GetUshort(row["AtkHPRcvDly"]),
                AtkHPRcv = GetDataTypes.GetUshort(row["AtkHPRcv"]),
                Str = GetDataTypes.GetUshort(row["Str"]),
                Dex = GetDataTypes.GetUshort(row["Dex"]),
                Con = GetDataTypes.GetUshort(row["Con"]),
                Int = GetDataTypes.GetUshort(row["Int"]),
                Men = GetDataTypes.GetUshort(row["Men"]),
                MobRaceType = GetDataTypes.GetUint(row["MobRaceType"]),
                Rank = GetDataTypes.GetByte(row["Rank"]),
                FamilyArea = GetDataTypes.GetUint(row["FamilyArea"]),
                FamilyRescArea = GetDataTypes.GetUint(row["FamilyRescArea"]),
                FamilyRescCount = GetDataTypes.GetByte(row["FamilyRescCount"]),
                BloodingResi = GetDataTypes.GetUshort(row["BloodingResi"]),
                StunResi = GetDataTypes.GetUshort(row["StunResi"]),
                MoveSpeedResi = GetDataTypes.GetUshort(row["MoveSpeedResi"]),
                FearResi = GetDataTypes.GetUshort(row["FearResi"]),
                ResIndex = (string) row["ResIndex"],
                KQKillPoint = GetDataTypes.GetUshort(row["KQKillPoint"]),
                Return2Regen = GetDataTypes.GetByte(row["Return2Regen"]),
                IsRoaming = GetDataTypes.GetByte(row["IsRoaming"]),
                RoamingNumber = GetDataTypes.GetByte(row["RoamingNumber"]),
                RoamingDistance = GetDataTypes.GetUshort(row["RoamingDistance"]),
                MaxSP = GetDataTypes.GetUshort(row["MaxSP"]),
                BroadAtDead = GetDataTypes.GetByte(row["BroadAtDead"]),
                TurnSpeed = GetDataTypes.GetUshort(row["TurnSpeed"]),
                WalkChase = GetDataTypes.GetUshort(row["WalkChase"]),
                AllCanLoot = GetDataTypes.GetByte(row["AllCanLoot"]),
                DmgByHealMin = GetDataTypes.GetUshort(row["DmgByHealMin"]),
                DmgByHealMax = GetDataTypes.GetUshort(row["DmgByHealMax"])
            };
            return info;
        }
    }
}