namespace Estrella.Zone
{
    public sealed class Settings
    {
        public const int SettingsVersion = 2;

        public int? Version { get; set; }
        public string IP { get; set; }
        public bool Debug { get; set; }
        public int WorkInterval { get; set; }
        public int TransferTimeout { get; set; }

        public string WorldServiceUri { get; set; }
        public string InterPassword { get; set; }
        public string WorldServerIP { get; set; }
        public ushort WorldServerPort { get; set; }
        public ushort InterServerPort { get; set; }
        public string zoneMysqlServer { get; set; }
        public int zoneMysqlPort { get; set; }
        public string zoneMysqlUser { get; set; }
        public string zoneMysqlPassword { get; set; }
        public string zoneMysqlDatabase { get; set; }
        public string WorldMysqlServer { get; set; }
        public int WorldMysqlPort { get; set; }
        public string WorldMysqlUser { get; set; }
        public string WorldMysqlPassword { get; set; }
        public string WorldMysqlDatabase { get; set; }
        public uint WorldDBMinPoolSizeZoneWorld { get; set; }
        public uint WorldDBMaxPoolSizeZoneWorld { get; set; }
        public static Settings Instance { get; set; }
        public string ConnString { get; set; }
        public string WorldConnString { get; set; }
        public uint ZoneDBMinPoolSize { get; set; }
        public uint ZoneDBMaxPoolSize { get; set; }
        public int OverloadFlags { get; set; }
        public int QueryCachePerClient { get; set; }
        public int OverloadFlagsZoneWorld { get; set; }
        public int QueryCachePerClientZoneWorld { get; set; }
        public ulong TicksToSleep { get; set; }
        public int SleepTime { get; set; }

        public static bool Load()
        {
            try
            {
                var obj = new Settings
                {
                    // V.1
                    WorldServerIP = InterLib.Settings.GetString("Zone.WorldServerIP"),
                    WorldServerPort = (ushort) InterLib.Settings.GetInt32("Zone.WorldServerPort"),
                    IP = InterLib.Settings.GetString("Zone.IP"),
                    Debug = InterLib.Settings.GetBool("Zone.Debug"),

                    WorkInterval = InterLib.Settings.GetInt32("Zone.WorkInterval"),
                    TransferTimeout = InterLib.Settings.GetInt32("Zone.TransferTimeout"),

                    WorldServiceUri = InterLib.Settings.GetString("Zone.WorldServiceURI"),
                    InterPassword = InterLib.Settings.GetString("Zone.Password"),
                    zoneMysqlServer = InterLib.Settings.GetString("Data.Mysql.Server"),
                    zoneMysqlPort = InterLib.Settings.GetInt32("Data.Mysql.Port"),
                    zoneMysqlUser = InterLib.Settings.GetString("Data.Mysql.User"),
                    zoneMysqlPassword = InterLib.Settings.GetString("Data.Mysql.Password"),
                    zoneMysqlDatabase = InterLib.Settings.GetString("Data.Mysql.Database"),
                    WorldMysqlServer = InterLib.Settings.GetString("World.Mysql.Server"),
                    ZoneDBMinPoolSize = (uint) InterLib.Settings.GetInt32("Data.Mysql.MinPool"),
                    ZoneDBMaxPoolSize = (uint) InterLib.Settings.GetInt32("Data.Mysql.MaxPool"),
                    WorldMysqlPort = InterLib.Settings.GetInt32("World.Mysql.Port"),
                    WorldMysqlUser = InterLib.Settings.GetString("World.Mysql.User"),
                    WorldMysqlPassword = InterLib.Settings.GetString("World.Mysql.Password"),
                    WorldMysqlDatabase = InterLib.Settings.GetString("World.Mysql.Database"),
                    QueryCachePerClientZoneWorld = InterLib.Settings.GetInt32("ZoneWorld.Mysql.QueryCachePerClient"),
                    OverloadFlagsZoneWorld = InterLib.Settings.GetInt32("ZoneWorld.Mysql.OverloadFlags"),
                    QueryCachePerClient = InterLib.Settings.GetInt32("Data.Mysql.QueryCachePerClient"),
                    OverloadFlags = InterLib.Settings.GetInt32("Data.Mysql.OverloadFlags"),
                    WorldDBMinPoolSizeZoneWorld = (uint) InterLib.Settings.GetInt32("ZoneWorld.Mysql.MinPool"),
                    WorldDBMaxPoolSizeZoneWorld = (uint) InterLib.Settings.GetInt32("ZoneWorld.Mysql.MaxPool"),
                    TicksToSleep = InterLib.Settings.GetUInt32("Zone.TicksToSleep"),
                    SleepTime = InterLib.Settings.GetInt32("Zone.SleepTime")
                };
                obj.WorldConnString = " User ID=" + obj.WorldMysqlUser + ";Password=" + obj.WorldMysqlPassword +
                                      ";Host=" + obj.WorldMysqlServer + ";Port=" + obj.WorldMysqlPort + ";Database=" +
                                      obj.WorldMysqlDatabase +
                                      ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
                obj.ConnString = " User ID=" + obj.zoneMysqlUser + ";Password=" + obj.zoneMysqlPassword + ";Host=" +
                                 obj.zoneMysqlServer + ";Port=" + obj.zoneMysqlPort + ";Database=" +
                                 obj.zoneMysqlDatabase +
                                 ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
                Instance = obj;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}