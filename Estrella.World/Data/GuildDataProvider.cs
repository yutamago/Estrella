using System;
using System.Collections.Generic;
using System.Data;
using Estrella.Util;
using Estrella.World.Data.Guild;
using MySql.Data.MySqlClient;

namespace Estrella.World.Data
{
    [ServerModule(InitializationStage.GuildProvider)]
    public class GuildDataProvider
    {
        public Dictionary<byte, uint> AcademyLevelUpPoints;

        public GuildDataProvider()
        {
            LoadAcademyLevelUpPonts();
            LoadGuilds();
        }

        public static GuildDataProvider Instance { get; set; }

        [InitializerMethod]
        public static bool Init()
        {
            Instance = new GuildDataProvider();
            Log.WriteLine(LogLevel.Info, "GuildDataProvider Initialsize");
            return true;
        }

        private void LoadGuilds()
        {
            var mysqlCmd = new MySqlCommand("SELECT * FROM Guilds",
                Program.DatabaseManager.GetClient().GetConnection());
            var GuildCount = 0;
            var GuildReader = mysqlCmd.ExecuteReader();
            {
                for (var i = 0; i < GuildReader.FieldCount; i++)
                {
                    while (GuildReader.Read())
                    {
                        var g = new Guild.Guild(Program.DatabaseManager.GetClient().GetConnection(), GuildReader);
                        GuildManager.AddGuildToList(g);
                        GuildCount++;
                    }
                }
            }
            GuildReader.Close();
            Log.WriteLine(LogLevel.Info, "Load {0} Guilds", GuildCount);
        }

        private void LoadAcademyLevelUpPonts()
        {
            AcademyLevelUpPoints = new Dictionary<byte, uint>();
            DataTable AcademyPoints = null;

            using (var dbClient = Program.DatabaseManager.GetClient())
            {
                AcademyPoints =
                    dbClient.ReadDataTable("SELECT * FROM `" + Settings.Instance.ZoneMysqlDatabase +
                                           "`.`AcademyLevelPoints`");
            }

            if (AcademyPoints != null)
            {
                foreach (DataRow row in AcademyPoints.Rows)
                {
                    AcademyLevelUpPoints.Add(Convert.ToByte(row["Level"]), Convert.ToUInt32(row["Points"]));
                }
            }

            Log.WriteLine(LogLevel.Info, "Load {0 } AcademyLevelUpPoints", AcademyLevelUpPoints.Count);
        }
    }
}