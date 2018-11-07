using System;
using System.IO;
using System.Xml.Serialization;
using Estrella.Util;

namespace Estrella.FiestaLib.Data
{
    //to update player stats, have to find out more later
    public sealed class BaseStats
    {
        public enum StatsByte : byte
        {
            MinMelee = 0x06,
            MaxMelee = 0x07,
            WDef = 0x08,

            Aim = 0x09,
            Evasion = 0x0a,

            MinMagic = 0x0b,
            MaxMagic = 0x0c,
            MDef = 0x0d,

            StrBonus = 0x13,
            EndBonus = 0x19
        }

        public readonly SerializableDictionary<byte, BaseStatsEntry> Entries =
            new SerializableDictionary<byte, BaseStatsEntry>();


        public BaseStats()
        {
        }

        public BaseStats(Job pJob)
        {
            Job = pJob;
        }


        public BaseStatsEntry this[byte level]
        {
            get
            {
                if (Entries.ContainsKey(level))
                    return Entries[level];
                return null;
            }
        }

        public Job Job { get; set; }

        public bool GetEntry(byte pLevel, out BaseStatsEntry pEntry)
        {
            return Entries.TryGetValue(pLevel, out pEntry);
        }

        public static bool TryLoad(string pFile, out BaseStats pStats)
        {
            pStats = new BaseStats();
            try
            {
                using (var file = File.Open(pFile, FileMode.Open))
                {
                    var xser = new XmlSerializer(typeof(BaseStats));
                    pStats = (BaseStats) xser.Deserialize(file);
                    // Log.WriteLine(LogLevel.Info, "Job {0} loaded! Data for {1} levels.", pStats.Job.ToString(), pStats.entries.Count);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Exception while loading stats from job {0}: {1}", pFile,
                    ex.ToString());
                return false;
            }
        }
    }
}