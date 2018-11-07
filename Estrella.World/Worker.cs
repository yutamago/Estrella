using System;
using System.Collections.Concurrent;
using System.Threading;
using Estrella.Util;
using Estrella.World.Managers;

namespace Estrella.World
{
    [ServerModule(InitializationStage.Worker)]
    public sealed class Worker
    {
        private readonly ConcurrentQueue<Action> _callbacks = new ConcurrentQueue<Action>();
        private readonly Thread _main;
        private int _sleep;
        private ulong TicksToSleep;

        public Worker()
        {
            _sleep = Settings.Instance.SleepTime;
            TicksToSleep = Settings.Instance.TicksToSleep;
            _main = new Thread(Work);
            IsRunning = true;
            _main.Start();
            new PerformCounter();
        }

        public static Worker Instance { get; private set; }
        public ulong TicksPerSecond { get; set; }

        public bool IsRunning { get; set; }

        [InitializerMethod]
        public static bool Load()
        {
            try
            {
                Instance = new Worker();
                Instance._sleep = Settings.Instance.WorkInterval;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddCallback(Action pCallback)
        {
            _callbacks.Enqueue(pCallback);
        }

        private void ConnectEntity()
        {
            /*    Program.Entity = EntityFactory.GetWorldEntity(Settings.Instance.Entity);
                // Try to update...
                DatabaseUpdater du = new DatabaseUpdater(Settings.Instance.Entity, DatabaseUpdater.DatabaseTypes.World);
                du.Update();*/
        }


        private void Work()
        {
            try
            {
                //ConnectEntity();
                //  Program.Entity.Characters.Count(); //test if database is online
                // Log.WriteLine(LogLevel.Info, "Database Initialized at {0}", Settings.Instance.Entity.DataCatalog);
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error initializing database: {0}", ex.ToString());
                return;
            }

            Action action;
            var pingCheckRan = DateTime.Now;
            var lastClientTime = DateTime.Now;
            ulong last = 0;
            var lastCheck = DateTime.Now;
            for (ulong i = 0;; i++)
            {
                if (!IsRunning) break;
                var now = DateTime.Now;

                while (_callbacks.TryDequeue(out action))
                {
                    try
                    {
                        var Work = new UserWorkItem(action);
                        Work.Queue();
                        //action();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(LogLevel.Exception, ex.ToString());
                    }
                }

                if (now.Subtract(pingCheckRan).TotalSeconds >= 15)
                {
                    // Just check every minute
                    ClientManager.Instance.PingCheck(now);
                    pingCheckRan = now;
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

                if (now.Subtract(lastClientTime).TotalSeconds >= 60)
                {
                    ClientManager.Instance.UpdateClientTime(now);
                    lastClientTime = now;
                }

                if (i % TicksToSleep == 0)
                {
                    Program.CurrentTime = DateTime.Now;
                    Thread.Sleep(_sleep);
                }
            }

            Log.WriteLine(LogLevel.Info, "Server stopped handling callbacks.");
        }
    }
}