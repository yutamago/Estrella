namespace Estrella.Login
{
    public sealed class Settings
    {
        public const int SettingsVersion = 2;

        public int? Version { get; set; }
        public ushort Port { get; set; }
        public bool Debug { get; set; }
        public int WorkInterval { get; set; }
        public string LoginMysqlServer { get; set; }
        public int LoginMysqlPort { get; set; }
        public string LoginMysqlUser { get; set; }
        public string LoginMysqlPassword { get; set; }
        public string LoginMysqlDatabase { get; set; }
        public uint LoginDBMinPoolSize { get; set; }
        public uint LoginDBMaxPoolSize { get; set; }
        public string LoginServiceUri { get; set; }
        public int OverloadFlags { get; set; }
        public int QueryCachePerClient { get; set; }
        public string InterPassword { get; set; }
        public ushort InterServerPort { get; set; }
        public static Settings Instance { get; set; }
        public string ConnString { get; set; }

        public static bool Load()
        {
            try
            {
                var obj = new Settings
                {
                    InterServerPort = (ushort) InterLib.Settings.GetInt32("Login.InterServerPort"),
                    Port = (ushort) InterLib.Settings.GetInt32("Login.Port"),
                    Debug = InterLib.Settings.GetBool("Login.Debug"),
                    WorkInterval = InterLib.Settings.GetInt32("Login.WorkInterVal"),
                    LoginServiceUri = InterLib.Settings.GetString("Login.LoginServiceURI"),
                    InterPassword = InterLib.Settings.GetString("Login.InterPassword"),
                    LoginMysqlServer = InterLib.Settings.GetString("Login.Mysql.Server"),
                    LoginMysqlPort = InterLib.Settings.GetInt32("Login.Mysql.Port"),
                    LoginMysqlUser = InterLib.Settings.GetString("Login.Mysql.User"),
                    LoginMysqlPassword = InterLib.Settings.GetString("Login.Mysql.Password"),
                    LoginMysqlDatabase = InterLib.Settings.GetString("Login.Mysql.Database"),
                    LoginDBMinPoolSize = InterLib.Settings.GetUInt32("Login.Mysql.MinPool"),
                    LoginDBMaxPoolSize = InterLib.Settings.GetUInt32("Login.Mysql.MaxPool"),
                    QueryCachePerClient = InterLib.Settings.GetInt32("Login.Mysql.QueryCachePerClient"),
                    OverloadFlags = InterLib.Settings.GetInt32("Login.Mysql.OverloadFlags"),

                    Version = SettingsVersion
                };
                obj.ConnString = " User ID=" + obj.LoginMysqlUser + ";Password=" + obj.LoginMysqlPassword + ";Host=" +
                                 obj.LoginMysqlServer + ";Port=" + obj.LoginMysqlPort + ";Database=" +
                                 obj.LoginMysqlDatabase + ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=" +
                                 obj.LoginDBMinPoolSize + ";Max Pool Size=" + obj.LoginDBMaxPoolSize +
                                 ";Connection Lifetime=0;";
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