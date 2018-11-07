using System;
using System.Collections.Generic;
using Estrella.Login.Networking;
using Estrella.Util;

namespace Estrella.Login
{
    [ServerModule(InitializationStage.Clients)]
    public sealed class ClientManager
    {
        private readonly List<LoginClient> clients = new List<LoginClient>();
        public static ClientManager Instance { get; private set; }

        public bool IsConnected(string ip)
        {
            lock (clients)
            {
                var client = clients.Find(c => c.Host == ip);
                return client != null;
            }
        }

        public bool IsLoggedIn(string username)
        {
            lock (clients)
            {
                var client = clients.Find(c => c.Username == username);
                return client != null;
            }
        }

        public bool RemoveClient(LoginClient client)
        {
            lock (clients)
            {
                return clients.Remove(client);
            }
        }


        public void AddClient(LoginClient client)
        {
            lock (clients)
            {
                clients.Add(client);
            }
        }

        [InitializerMethod]
        public static bool Load()
        {
            try
            {
                Instance = new ClientManager();
                Log.WriteLine(LogLevel.Info, "ClientManager Initialized.");
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteLine(LogLevel.Exception, "ClientManager failed to initialize: {0}", exception.ToString());
                return false;
            }
        }
    }
}