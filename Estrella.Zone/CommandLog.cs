﻿using System;
using System.IO;
using Estrella.Util;

namespace Estrella.Zone
{
    [ServerModule(InitializationStage.DataStore)]
    public sealed class CommandLog
    {
        private readonly StreamWriter writer;

        public CommandLog(string filename)
        {
            writer = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            writer.AutoFlush = true;
        }

        public static CommandLog Instance { get; private set; }

        [InitializerMethod]
        public static bool Load()
        {
            try
            {
                Instance = new CommandLog("AdminLog.txt");
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error initializing commandlog: {0}", ex.ToString());
                return false;
            }
        }

        public void LogCommand(string username, string command)
        {
            writer.WriteLine("[{0}] {1} : {2}", Program.CurrentTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"), username,
                command);
        }
    }
}