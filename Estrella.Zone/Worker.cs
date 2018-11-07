using System;
using System.Collections.Concurrent;
using System.Threading;
using Estrella.Util;
using Estrella.Zone.Managers;

namespace Estrella.Zone
{
    [ServerModule(InitializationStage.DataStore)]
    public sealed class Worker
    {
        private readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
        private readonly Thread main;
        private int sleep = 1;
        private ulong ticksToSleep = 1500;

        public Worker()
        {
            sleep = Settings.Instance.SleepTime;
            ticksToSleep = Settings.Instance.TicksToSleep;
            main = new Thread(Work);
            TicksPerSecond = 0;
            IsRunning = true;
            main.Start();
            new PerformCounter();
        }

        public static Worker Instance { get; private set; }
        public ulong TicksPerSecond { get; set; }
        public bool IsRunning { get; set; }

        public static bool Load()
        {
            try
            {
                Instance = new Worker();
                Instance.sleep = Settings.Instance.WorkInterval;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddCallback(Action pCallback)
        {
            callbacks.Enqueue(pCallback);
        }


        public void Stop()
        {
            main?.Abort();
        }

        private void Work()
        {
            while (Program.ServiceInfo == null)
            {
                Thread.Sleep(200); // Wait..
            }

            try
            {
                // Estrella.Database.DatabaseHelper.Initialize(Settings.Instance.WorldConnString, "WorkerConn");
                //  Program.Entity.Characters.Count(); //test if database is online
                Log.WriteLine(LogLevel.Info, "Database Initialized.");
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error initializing database: {0}", ex.ToString());
                return;
            }

            Action action;
            ulong last = 0;
            var lastCheck = DateTime.Now;
            var lastPing = DateTime.Now;
            var lastGC = DateTime.Now;
            var lastClientTime = DateTime.Now;
            var LastMountCheck = DateTime.Now;
            for (ulong i = 0;; i++)
            {
                if (!IsRunning)
                {
                    break;
                }

                try
                {
                    var now = Program.CurrentTime;

                    while (callbacks.TryDequeue(out action))
                    {
                        try
                        {
                            var Work = new UserWorkItem(action);
                            Work.Queue();
                            //  action();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(LogLevel.Exception, ex.ToString());
                        }
                    }

                    if (now.Subtract(lastCheck).TotalSeconds >= 1)
                    {
                        TicksPerSecond = i - last;
                        last = i;
                        lastCheck = now;
                        //Log.WriteLine(LogLevel.Debug, "TicksPerSecond: {0}", TicksPerSecond);
                        if (TicksPerSecond <= 100)
                        {
                            Log.WriteLine(LogLevel.Warn, "Server overload! Only {0} ticks per second!", TicksPerSecond);
                        }
                    }

                    if (now.Subtract(lastPing).TotalSeconds >= 300)
                    {
                        ClientManager.Instance.PingCheck();
                        lastPing = now;
                    }

                    if (now.Subtract(lastGC).TotalSeconds >= 300)
                    {
                        GC.Collect();
                        lastGC = now;
                    }

                    if (now.Subtract(lastClientTime).TotalSeconds >= 60)
                    {
                    }

                    if (now.Subtract(LastMountCheck).TotalSeconds >= 30)
                    {
                        ClientManager.Instance.UpdateMountTicks(now);
                        LastMountCheck = now;
                    }

                    if (i % 2000 == 0 && MapManager.Instance != null)
                    {
                        foreach (var val in MapManager.Instance.Maps)
                        {
                            foreach (var map in val.Value)
                            {
                                map.Update(now); //test
                            }
                        }
                    }

                    if (i % ticksToSleep == 0) // No max load but most ticks to be parsed. Epic win!
                    {
                        Program.CurrentTime = DateTime.Now; // Laaast update
                        Thread.Sleep(sleep);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogLevel.Exception, "Ohgod. {0}", ex.ToString());
                }
            }

            Log.WriteLine(LogLevel.Info, "Server stopped handling callbacks.");
        }
    }
}