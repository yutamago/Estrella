using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using Estrella.Database;
using Estrella.Util;
using Estrella.World.InterServer;

namespace Estrella.World
{
    internal static class Program
    {
        private static bool _handleCommands = true;
        public static bool Maintenance { get; private set; }
        public static DatabaseManager DatabaseManager { get; private set; }
        public static DateTime CurrentTime { get; set; }
        public static ConcurrentDictionary<byte, ZoneConnection> Zones { get; private set; }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;
            Console.Title = "Estrella.World";
#if DEBUG
            Thread.Sleep(980); //give loginserver some time to start.
#endif
            if (Load())
            {
                Log.IsDebug = Settings.Instance.Debug;
                Zones = new ConcurrentDictionary<byte, ZoneConnection>();


                while (_handleCommands)
                {
                    var line = Console.ReadLine();
                    try
                    {
                        HandleCommand(line);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(LogLevel.Exception, "Could not parse: {0}; Error: {1}", line, ex.ToString());
                    }
                }

                Log.WriteLine(LogLevel.Warn, "Shutting down the server..");
                CleanUp();
                Log.WriteLine(LogLevel.Info, "Server has been cleaned up. Program will now exit.");
            }
            else
            {
                Log.WriteLine(LogLevel.Error, "Errors occured starting server. Press RETURN to exit.");
                Console.ReadLine();
            }
        }

        private static void CleanUp()
        {
            foreach (var method in Reflector.GetCleanupMethods())
            {
                method();
            }
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception) args.ExceptionObject;

            #region Logging

            #region Write Errors to a log file

            // Create a writer and open the file:

            var log = !File.Exists("errorlog.txt") ? new StreamWriter("errorlog.txt") : File.AppendText("errorlog.txt");

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

        public static ZoneConnection GetZoneByMap(int id)
        {
            try
            {
                return Zones.Values.First(z => z.Maps.Count(m => m.ID == id) > 0);
            }
            catch
            {
                Log.WriteLine(LogLevel.Exception, "No zones are active at the moment.");
                return null;
            }
        }

        public static ZoneConnection GetZoneByMapShortName(string name)
        {
            try
            {
                return Zones.Values.First(z => z.Maps.Count(m => m.ShortName == name) > 0);
            }
            catch
            {
                Log.WriteLine(LogLevel.Exception, "No zones are active at the moment.");
                return null;
            }
        }

        private static void HandleCommand(string line)
        {
            var command = line.Split(' ');
            switch (command[0].ToLower())
            {
                case "maintenance":
                    if (command.Length >= 2)
                    {
                        Maintenance = bool.Parse(command[1]);
                    }

                    break;
                case "shutdown":
                    _handleCommands = false;
                    break;
                case "exit":
                    _handleCommands = false;
                    break;
                case "quit":
                    _handleCommands = false;
                    break;
                default:
                    Console.WriteLine("Command not recognized.");
                    break;
            }
        }

        private static bool Load()
        {
            InterLib.Settings.Initialize();
            Settings.Load();
            DatabaseManager = new DatabaseManager(Settings.Instance.WorldMysqlServer,
                (uint) Settings.Instance.WorldMysqlPort, Settings.Instance.WorldMysqlUser,
                Settings.Instance.WorldMysqlPassword, Settings.Instance.WorldMysqlDatabase,
                Settings.Instance.WorldDbMinPoolSize, Settings.Instance.WorldDbMaxPoolSize,
                Settings.Instance.QueryCachePerClient, Settings.Instance.OverloadFlags);
            //DatabaseManager.GetClient(); //testclient
            Log.SetLogToFile($@"Logs\World\{DateTime.Now:d_M_yyyy HH_mm_ss}.log");

            try
            {
                if (Reflector.GetInitializerMethods().All(method => method.Invoke())) return true;
                Log.WriteLine(LogLevel.Error, "Server could not be started. Errors occured.");
                return false;

            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Fatal exception while load: {0}:{1}", ex.ToString(), ex.StackTrace);
                return false;
            }
        }

        public static byte GetFreeZoneId()
        {
            for (byte i = 0; i < 3; i++)
            {
                if (Zones.ContainsKey(i)) continue;
                return i;
            }

            return 255;
        }
    }
}