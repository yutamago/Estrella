﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using Estrella.Database;
using Estrella.FiestaLib.Data;
using Estrella.Util;
using Estrella.Zone.InterServer;
using Estrella.Zone.Managers;
using Estrella.Zone.Networking;

namespace Estrella.Zone
{
    internal static class Program
    {
        public static DatabaseManager DatabaseManager;
        public static DatabaseManager CharDBManager;

        public static ZoneData ServiceInfo
        {
            get => Zones[0];
            set => Zones[0] = value;
        }

        public static ConcurrentDictionary<byte, ZoneData> Zones { get; set; }

        //public static WorldEntity Entity { get; set; }
        public static Random Randomizer { get; set; }
        public static DateTime CurrentTime { get; set; }
        public static bool Shutdown { get; private set; }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;
            Console.Title = "Estrella.Zone[Registering]";
            Console.WindowWidth = 90;
#if DEBUG
            // so the startup works
            Thread.Sleep(TimeSpan.FromSeconds(3));
#endif
            Zones = new ConcurrentDictionary<byte, ZoneData>();
            Zones.TryAdd(0, new ZoneData());
            if (Load())
            {
                // Start Worker thread.
                Worker.Load();
                Worker.Instance.AddCallback(GroupManager.Instance.Update);
                while (true)
                {
                    var cmd = Console.ReadLine();
                    var arguments = cmd.Split(' ');
                    switch (arguments[0])
                    {
                        case "shutdown":
                            Shutdown = true;
                            Log.WriteLine(LogLevel.Info, "Disconnecting from world.");
                            WorldConnector.Instance.Disconnect();
                            Log.WriteLine(LogLevel.Info, "Stopping client acceptor");
                            ZoneAcceptor.Instance.Stop();
                            Log.WriteLine(LogLevel.Info, "Stopping worker thread");
                            Worker.Instance.Stop();
                            Log.WriteLine(LogLevel.Info, "Disconnecting all clients");
                            ClientManager.Instance.DisconnectAll();
                            Log.WriteLine(LogLevel.Info, "Saving everything a last time");
                            // Entity.SaveChanges();
                            Log.WriteLine(LogLevel.Info, "Bay.");
                            Environment.Exit(1);
                            break;
                    }
                }
            }

            Console.WriteLine("There was an error during load. Please press RETURN to exit.");
            Console.ReadLine();
        }

        private static bool Load()
        {
            InterLib.Settings.Initialize();
            Settings.Load();
            DatabaseManager = new DatabaseManager(Settings.Instance.zoneMysqlServer,
                (uint) Settings.Instance.zoneMysqlPort, Settings.Instance.zoneMysqlUser,
                Settings.Instance.zoneMysqlPassword, Settings.Instance.zoneMysqlDatabase,
                Settings.Instance.ZoneDBMinPoolSize, Settings.Instance.ZoneDBMaxPoolSize, 10, 1);
            DatabaseManager.GetClient(); //testclient   
            CharDBManager = new DatabaseManager(Settings.Instance.WorldMysqlServer,
                (uint) Settings.Instance.WorldMysqlPort, Settings.Instance.WorldMysqlUser,
                Settings.Instance.WorldMysqlPassword, Settings.Instance.WorldMysqlDatabase,
                Settings.Instance.WorldDBMinPoolSizeZoneWorld, Settings.Instance.WorldDBMaxPoolSizeZoneWorld,
                Settings.Instance.QueryCachePerClientZoneWorld, Settings.Instance.OverloadFlagsZoneWorld);
            CharDBManager.GetClient();
            Log.SetLogToFile($@"Logs\Zone\{DateTime.Now:yyyy-MM-dd HHmmss}.log");
            Randomizer = new Random();
            Log.IsDebug = Settings.Instance.Debug;

            try
            {
                if (Reflector.GetInitializerMethods().Any(method => !method.Invoke()))
                {
                    Log.WriteLine(LogLevel.Error, "Server could not be started. Errors occured.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error loading Initializer methods: {0}", ex.ToString());
                return false;
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception) args.ExceptionObject;

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

            Log.WriteLine(LogLevel.Exception, "Unhandled Exception : " + e);
            Console.ReadKey(true);
        }

        public static ZoneData GetZoneForMap(ushort mapid)
        {
            return Zones.Values.FirstOrDefault(v => v.MapsToLoad.Count(m => m.ID == mapid) > 0);
        }

        public static MapInfo GetMapInfo(ushort mapid)
        {
            return Zones.Values.Select(v => v.MapsToLoad.Find(m => m.ID == mapid)).FirstOrDefault(mi => mi != null);
        }

        public static bool IsLoaded(ushort mapid)
        {
            try
            {
                return ServiceInfo.MapsToLoad.Count(m => m.ID == mapid) > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}