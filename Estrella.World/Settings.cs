namespace Estrella.World
{
    public sealed class Settings
    {
        public const int SettingsVersion = 2;

        public int? Version { get; set; }
        public string WorldName { get; set; }
        public byte Id { get; set; }
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public ushort ZoneBasePort { get; set; }
        public ushort ZoneCount { get; set; }

        public int TransferTimeout { get; set; }
        public bool Debug { get; set; }
        public int WorkInterval { get; set; }

        public string LoginServiceUri { get; set; }
        public string WorldServiceUri { get; set; }
        public string GameServiceUri { get; set; }
        public string InterPassword { get; set; }
        public string LoginServerIp { get; set; }
        public ushort LoginServerPort { get; set; }
        public ushort InterServerPort { get; set; }
        public string WorldMysqlServer { get; set; }
        public int WorldMysqlPort { get; set; }
        public string WorldMysqlUser { get; set; }
        public string WorldMysqlPassword { get; set; }
        public string WorldMysqlDatabase { get; set; }
        public string ZoneMysqlServer { get; set; }
        public int ZoneMysqlPort { get; set; }
        public string ZoneMysqlUser { get; set; }
        public string ZoneMysqlPassword { get; set; }
        public string ZoneMysqlDatabase { get; set; }
        public bool ShowEquips { get; set; }
        public string ConnString { get; set; }
        public string DataConnString { get; set; }
        public static Settings Instance { get; set; }
        public uint WorldDbMinPoolSize { get; set; }
        public uint WorldDbMaxPoolSize { get; set; }
        public int OverloadFlags { get; set; }
        public int QueryCachePerClient { get; set; }
        public ulong TicksToSleep { get; set; }
        public int SleepTime { get; set; }

        public static bool Load()
        {
            try
            {
                var obj = new Settings
                {
                    Port = (ushort) InterLib.Settings.GetInt32("World.Port"),
                    ZoneBasePort = (ushort) InterLib.Settings.GetInt32("World.ZoneBase.Port"),
                    ZoneCount = (ushort) InterLib.Settings.GetInt32("World.ZoneCount"),
                    Ip = InterLib.Settings.GetString("World.IP"),
                    Debug = InterLib.Settings.GetBool("World.Debug"),
                    InterServerPort = (ushort) InterLib.Settings.GetInt32("World.InterServerPort"), //zone listener port
                    WorkInterval = InterLib.Settings.GetInt32("World.WorkInterval"),
                    TransferTimeout = InterLib.Settings.GetInt32("World.TransferTimeout"),
                    LoginServerIp = InterLib.Settings.GetString("World.LoginServer.IP"),
                    LoginServerPort = (ushort) InterLib.Settings.GetInt32("World.LoginServer.Port"),

                    WorldName = InterLib.Settings.GetString("World.Name"),
                    Id = InterLib.Settings.GetByte("World.ID"),
                    ShowEquips = true,
                    LoginServiceUri = InterLib.Settings.GetString("World.LoginServiceURI"),
                    WorldServiceUri = InterLib.Settings.GetString("World.WorldServiceURI"),
                    GameServiceUri = InterLib.Settings.GetString("World.GameServiceURI"),
                    InterPassword = InterLib.Settings.GetString("World.InterPassword"),
                    WorldMysqlServer = InterLib.Settings.GetString("World.Mysql.Server"),
                    WorldMysqlPort = InterLib.Settings.GetInt32("World.Mysql.Port"),
                    WorldMysqlUser = InterLib.Settings.GetString("World.Mysql.User"),
                    WorldMysqlPassword = InterLib.Settings.GetString("World.Mysql.Password"),
                    WorldMysqlDatabase = InterLib.Settings.GetString("World.Mysql.Database"),
                    ZoneMysqlServer = InterLib.Settings.GetString("Data.Mysql.Server"),
                    ZoneMysqlPort = InterLib.Settings.GetInt32("Data.Mysql.Port"),
                    ZoneMysqlUser = InterLib.Settings.GetString("Data.Mysql.User"),
                    ZoneMysqlPassword = InterLib.Settings.GetString("Data.Mysql.Password"),
                    ZoneMysqlDatabase = InterLib.Settings.GetString("Data.Mysql.Database"),
                    WorldDbMinPoolSize = InterLib.Settings.GetUInt32("World.Mysql.MinPool"),
                    WorldDbMaxPoolSize = InterLib.Settings.GetUInt32("World.Mysql.MaxPool"),
                    QueryCachePerClient = InterLib.Settings.GetInt32("World.Mysql.QueryCachePerClient"),
                    OverloadFlags = InterLib.Settings.GetInt32("World.Mysql.OverloadFlags"),
                    TicksToSleep = InterLib.Settings.GetUInt32("World.TicksToSleep"),
                    SleepTime = InterLib.Settings.GetInt32("World.SleepTime")
                };
                obj.ConnString = " User ID=" + obj.WorldMysqlUser + ";Password=" + obj.WorldMysqlPassword + ";Host=" +
                                 obj.WorldMysqlServer + ";Port=" + obj.WorldMysqlPort + ";Database=" +
                                 obj.WorldMysqlDatabase +
                                 ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
                obj.DataConnString = " User ID=" + obj.ZoneMysqlUser + ";Password=" + obj.ZoneMysqlPassword + ";Host=" +
                                     obj.ZoneMysqlServer + ";Port=" + obj.ZoneMysqlPort + ";Database=" +
                                     obj.ZoneMysqlDatabase +
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