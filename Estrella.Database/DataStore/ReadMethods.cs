using System;
using System.Data;
using Estrella.Database.Storage;
using Estrella.Util;
using MySql.Data.MySqlClient;

namespace Estrella.Database.DataStore
{
    public class ReadMethods
    {
        private const string SelectQuickBar = "SELECT QuickBar FROM characters WHERE CharID=@characterID";
        private const string SelectShortcuts = "SELECT Shortcuts FROM characters WHERE CharID=@characterID";
        private const string SelectQuickBarState = "SELECT QuickBarState FROM characters WHERE CharID=@characterID";
        private const string SelectClientSettings = "SELECT ClientSettings FROM characters WHERE CharID=@characterID";
        private const string SelectGameSettings = "SELECT ClientSettings FROM characters WHERE CharID=@characterID";

        public static bool EnumToBool(string @enum)
        {
            if (@enum == "1")
            {
                return true;
            }

            return false;
        }

        public static Character ReadCharObjectByIDFromDatabase(int ID, DatabaseManager dbmanager)
        {
            var ch = new Character();
            DataTable charData = null;
            using (var dbClient = dbmanager.GetClient())
            {
                charData = dbClient.ReadDataTable("SELECT  *FROM `characters` WHERE  CharID= '" + ID + "'");
            }

            if (charData != null)
            {
                foreach (DataRow row in charData.Rows)
                {
                    ch.PositionInfo.ReadFromDatabase(row);
                    var ne = DateTime.Now;
                    ch.AccountID = GetDataTypes.GetInt(row["AccountID"]);
                    ch.LookInfo.ReadFromDatabase(row);
                    ch.CharacterStats.ReadFromDatabase(row);
                    ch.Slot = (byte) row["Slot"];
                    ch.CharLevel = (byte) row["Level"];
                    ch.Name = (string) row["Name"];
                    ch.ID = GetDataTypes.GetInt(row["CharID"]);
                    ch.Job = (byte) row["Job"];
                    ch.Money = GetDataTypes.GetLong(row["Money"]);
                    ch.Exp = GetDataTypes.GetLong(row["Exp"]);
                    ch.HP = GetDataTypes.GetInt(row["CurHP"]);
                    ch.HPStones = 10;
                    ch.GuildID = GetDataTypes.GetInt(row["GuildID"]);
                    ch.AcademyID = GetDataTypes.GetInt(row["AcademyID"]);
                    ch.SP = GetDataTypes.GetInt(row["CurSP"]);
                    ch.SPStones = 10;
                    ch.MountFood = GetDataTypes.GetInt(row["MountFood"]);
                    ch.MountID = GetDataTypes.GetInt(row["MountID"]);
                    ch.StatPoints = (byte) row["StatPoints"];
                    ch.UsablePoints = (byte) row["UsablePoints"];
                    ch.MasterJoin = DateTime.Parse(row["MasterJoin"].ToString());
                    ch.Fame = 0;
                    ch.GameSettings = GetGameSettings(ch.ID, dbmanager);
                    ch.ClientSettings = GetClientSettings(ch.ID, dbmanager);
                    ch.Shortcuts = GetShortcuts(ch.ID, dbmanager);
                    ch.QuickBar = GetQuickBar(ch.ID, dbmanager);
                    ch.QuickBarState = GetQuickBarState(ch.ID, dbmanager);
                }
            }

            try
            {
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return ch;
        }

        public static byte[] GetQuickBar(int charID, DatabaseManager dbManager)
        {
            try
            {
                var command = new MySqlCommand(SelectQuickBar);
                byte[] quickbar;
                using (var dbClient = dbManager.GetClient())
                {
                    command.Connection = dbClient.GetConnection();
                    command.Parameters.AddWithValue("@characterID", charID);
                    quickbar = dbClient.GetBlob(command);
                }

                //byte[] Quickbar = DbManager.GetClient().GetBlob(command);
                return quickbar;
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "GetQuickbar Failed from datatbase");
                return null;
            }
        }

        public static byte[] GetShortcuts(int charID, DatabaseManager dbManager)
        {
            try
            {
                var command = new MySqlCommand(SelectShortcuts);
                byte[] shortcuts;
                using (var dbClient = dbManager.GetClient())
                {
                    command.Connection = dbClient.GetConnection();
                    command.Parameters.AddWithValue("@characterID", charID);
                    shortcuts = dbClient.GetBlob(command);
                }


                return shortcuts;
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "GetShortcuts Failed from datatbase");
                return null;
            }
        }

        public static byte[] GetQuickBarState(int charID, DatabaseManager dbManager)
        {
            try
            {
                var command = new MySqlCommand(SelectQuickBarState);
                byte[] quickBarState;
                using (var dbClient = dbManager.GetClient())
                {
                    command.Connection = dbClient.GetConnection();
                    command.Parameters.AddWithValue("@characterID", charID);
                    quickBarState = dbClient.GetBlob(command);
                }

                return quickBarState;
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "GetQuickBarState Failed from datatbase");
                return null;
            }
        }

        public static byte[] GetGameSettings(int charID, DatabaseManager dbManager)
        {
            try
            {
                var command = new MySqlCommand(SelectGameSettings);
                byte[] gameSettings;
                using (var dbClient = dbManager.GetClient())
                {
                    command.Connection = dbClient.GetConnection();
                    command.Parameters.AddWithValue("@characterID", charID);
                    gameSettings = dbClient.GetBlob(command);
                }

                return gameSettings;
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "GetGameSettings Failed from datatbase");
                return null;
            }
        }

        public static byte[] GetClientSettings(int charID, DatabaseManager dbManager)
        {
            try
            {
                var command = new MySqlCommand(SelectClientSettings);
                byte[] clientSetting;
                using (var dbClient = dbManager.GetClient())
                {
                    command.Connection = dbClient.GetConnection();
                    command.Parameters.AddWithValue("@characterID", charID);
                    clientSetting = dbClient.GetBlob(command);
                }

                return clientSetting;
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "GetClientSettings Failed from datatbase");
                return null;
            }
        }
    }
}