using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using Estrella.Database;
using Estrella.Util;

namespace Estrella.Login
{
    class Program
    {
        internal static DatabaseManager DatabaseManager { get; set; }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;
            //if debug we always start with default settings :)
#if DEBUG
            //File.Delete("Login.xml");
#endif

            Console.Title = "Estrella.Login";
            if (Load())
            {
                Log.IsDebug = Settings.Instance.Debug;
                while (true)
                    Console.ReadLine();
            }

            Log.WriteLine(LogLevel.Error, "Could not start server. Press RETURN to exit.");
            Console.ReadLine();
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception) args.ExceptionObject;

            #region Logging

            #region Write Errors to a log file

            // Create a writer and open the file:
            StreamWriter log;

            if (!File.Exists("errorlog.txt"))
            {
                log = new StreamWriter("errorlog.txt");
            }
            else
            {
                log = File.AppendText("errorlog.txt");
            }

            // Write to the file:
            log.WriteLine(DateTime.Now);
            log.WriteLine(e.ToString());
            log.WriteLine();

            // Close the stream:
            log.Close();

            #endregion

            #endregion

            Log.WriteLine(LogLevel.Exception, "Unhandled Exception : " + e);
            Console.ReadKey(true);
        }

        public static bool Load()
        {
            InterLib.Settings.Initialize();
            Settings.Load();
            DatabaseManager = new DatabaseManager(Settings.Instance.LoginMysqlServer,
                (uint) Settings.Instance.LoginMysqlPort, Settings.Instance.LoginMysqlUser,
                Settings.Instance.LoginMysqlPassword, Settings.Instance.LoginMysqlDatabase,
                Settings.Instance.LoginDBMinPoolSize, Settings.Instance.LoginDBMaxPoolSize,
                Settings.Instance.QueryCachePerClient, Settings.Instance.OverloadFlags);
            DatabaseManager.GetClient(); //testclient

            Log.SetLogToFile($@"Logs\Login\{DateTime.Now.ToString("d_M_yyyy HH_mm_ss")}.log");

            if (Reflector.GetInitializerMethods().Any(method => !method.Invoke()))
            {
                Log.WriteLine(LogLevel.Error, "Server could not be started. Errors occured.");
                return false;
            }

            return true;
        }
    }
}