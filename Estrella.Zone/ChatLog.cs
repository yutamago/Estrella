using System;
using System.IO;
using Estrella.Util;

namespace Estrella.Zone
{
    [ServerModule(InitializationStage.DataStore)]
    public sealed class ChatLog
    {
        private readonly StreamWriter _writer;

        private ChatLog(string filename)
        {
            _writer = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                AutoFlush = true
            };
        }

        public static ChatLog Instance { get; private set; }

        [InitializerMethod]
        public static bool Load()
        {
            try
            {
                Instance = new ChatLog("ChatLog.txt");
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error initializing chatlog: {0}", ex.ToString());
                return false;
            }
        }

        public void LogChat(string username, string line, bool shout)
        {
            _writer.WriteLine(
                shout
                    ? "[{0:yyyy\'-\'MM\'-\'dd\' \'HH\':\'mm\':\'ss}][SHOUT] {1} : {2}"
                    : "[{0:yyyy\'-\'MM\'-\'dd\' \'HH\':\'mm\':\'ss}] {1} : {2}", Program.CurrentTime, username, line);
        }
    }
}