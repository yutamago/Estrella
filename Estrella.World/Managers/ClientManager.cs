using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Handlers;
using Estrella.World.Networking;
using Timer = System.Timers.Timer;

namespace Estrella.World.Managers
{
    [ServerModule(InitializationStage.Clients)]
    public sealed class ClientManager
    {
        private readonly List<WorldClient> _clients = new List<WorldClient>();

        private readonly ConcurrentDictionary<int, WorldClient> _clientsByCharId =
            new ConcurrentDictionary<int, WorldClient>();

        private readonly ConcurrentDictionary<string, WorldClient> _clientsByName =
            new ConcurrentDictionary<string, WorldClient>();

        private readonly Timer _expirator;
        readonly List<WorldClient> _pingTimeouts = new List<WorldClient>();

        private readonly List<string> _toExpire = new List<string>();

        private readonly ConcurrentDictionary<string, ClientTransfer> _transfers =
            new ConcurrentDictionary<string, ClientTransfer>();

        private readonly ConcurrentDictionary<string, WorldClient> _zoneAdd =
            new ConcurrentDictionary<string, WorldClient>();

        private Mutex _clientManagerMutex = new Mutex();
        private int _transferTimeout = 1;

        public ClientManager()
        {
            _expirator = new Timer(2000);
            _expirator.Elapsed += ExpiratorElapsed;
            _expirator.Start();
        }

        public static ClientManager Instance { get; private set; }

        public int WorldLoad
        {
            get { return ClientCount(); }
        }

        private int ClientCount()
        {
            lock (_clients)
            {
                return _clients.Count;
            }
        }

        public void AddClient(WorldClient client)
        {
            _clientManagerMutex.WaitOne();
            try
            {
                _clients.Add(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _clientManagerMutex.ReleaseMutex();
            }
        }

        public void UpdateClientTime(DateTime dateTime)
        {
            lock (_clients)
            {
                foreach (var kvp in _clientsByName.Values)
                {
                    Handler2.SendClientTime(kvp, dateTime);
                }
            }
        }

        public void AddClientByName(WorldClient client)
        {
            _clientManagerMutex.WaitOne();
            try
            {
                if (client.Character != null && !_clientsByName.ContainsKey(client.Character.Character.Name))
                {
                    _clientsByCharId.TryAdd(client.Character.Character.ID, client);
                    _clientsByName.TryAdd(client.Character.Character.Name, client);
                }
                else Log.WriteLine(LogLevel.Warn, "Trying to register client by name without having Character object.");
            }
            finally
            {
                _clientManagerMutex.ReleaseMutex();
            }
        }

        public void AddZoneTrans(string name, WorldClient client)
        {
            _clientManagerMutex.WaitOne();
            try
            {
                _zoneAdd.TryAdd(name, client);
            }
            finally
            {
                _clientManagerMutex.ReleaseMutex();
            }
        }

        public void RemoveZoneTrand(string name, WorldClient ccclient)
        {
            try
            {
                _zoneAdd.TryRemove(name, out ccclient);
            }
            finally
            {
                _clientManagerMutex.ReleaseMutex();
            }
        }

        public void PingCheck(DateTime now)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (!client.Authenticated)
                        continue; //they don't have ping shit, since they don't even send a response.
                    if (client.Pong)
                    {
                        Handler2.SendPing(client);
                        client.Pong = false;
                        client.LastPing = now;
                    }
                    else
                    {
                        if (now.Subtract(client.LastPing).TotalSeconds >= 300)
                        {
                            _pingTimeouts.Add(client);
                            Log.WriteLine(LogLevel.Debug, "Ping timeout from {0} ({1})", client.Username, client.Host);
                        }
                    }
                }

                foreach (var client in _pingTimeouts)
                {
                    _clients.Remove(client);
                    client.Disconnect();
                }

                _pingTimeouts.Clear();
            }
        }

        public WorldClient GetClientByCharname(string name)
        {
            WorldClient client;
            if (_clientsByName.TryGetValue(name, out client))
            {
                return client;
            }

            return null;
        }

        public WorldClient GetClientByCharId(int id)
        {
            WorldClient client;
            if (_clientsByCharId.TryGetValue(id, out client))
            {
                return client;
            }

            return null;
        }

        public void RemoveClient(WorldClient client)
        {
            _clientManagerMutex.WaitOne();
            try
            {
                _clients.Remove(client);


                if (client.Character != null)
                {
                    WorldClient deleted;
                    _clientsByName.TryRemove(client.Character.Character.Name, out deleted);
                    if (deleted != client)
                    {
                        Log.WriteLine(LogLevel.Warn, "There was a duplicate client in clientsByName: {0}",
                            client.Character.Character.Name);
                    }
                }
            }
            finally
            {
                _clientManagerMutex.ReleaseMutex();
            }
        }

        public void AddTransfer(ClientTransfer transfer)
        {
            if (transfer.Type != TransferType.World)
            {
                Log.WriteLine(LogLevel.Warn, "Received a GAME transfer request. Trashing it.");
                return;
            }

            if (_transfers.ContainsKey(transfer.Hash))
            {
                ClientTransfer trans;
                if (_transfers.TryRemove(transfer.Hash, out trans))
                {
                    Log.WriteLine(LogLevel.Warn, "Duplicate client transfer hash. {0} hacked into {1}", transfer.HostIP,
                        trans.HostIP);
                }
            }

            if (!_transfers.TryAdd(transfer.Hash, transfer))
            {
                Log.WriteLine(LogLevel.Warn, "Error registering client transfer.");
            }
        }

        public bool RemoveTransfer(string hash)
        {
            ClientTransfer trans;
            return _transfers.TryRemove(hash, out trans);
        }

        public ClientTransfer GetTransfer(string hash)
        {
            ClientTransfer trans;
            if (_transfers.TryGetValue(hash, out trans))
            {
                return trans;
            }

            return null;
        }

        public void SendPacketToAll(Packet pPacket, WorldClient pExcept = null)
        {
            foreach (var client in _clients.FindAll(c => c != pExcept))
            {
                client.SendPacket(pPacket);
            }
        }

        void ExpiratorElapsed(object sender, ElapsedEventArgs e)
        {
            //this is actually executed in the main thread! (ctor is in STAThread)
            foreach (var transfer in _transfers.Values)
            {
                if ((DateTime.Now - transfer.Time).TotalMilliseconds >= _transferTimeout)
                {
                    _toExpire.Add(transfer.Hash);
                    Log.WriteLine(LogLevel.Debug, "Transfer timeout for {0}", transfer.Username);
                }
            }

            if (_toExpire.Count > 0)
            {
                foreach (var expired in _toExpire)
                {
                    ClientTransfer trans;
                    _transfers.TryRemove(expired, out trans);
                }

                _toExpire.Clear();
            }
        }

        public bool IsOnline(string pCharname)
        {
            return _clientsByName.ContainsKey(pCharname);
        }

        [InitializerMethod]
        public static bool Load()
        {
            Instance = new ClientManager
            {
                _transferTimeout = Settings.Instance.TransferTimeout
            };
            Log.WriteLine(LogLevel.Info, "ClientManager initialized.");
            return true;
        }

        [CleanUpMethod]
        public static void CleanUp()
        {
            Log.WriteLine(LogLevel.Info, "Cleaning up ClientManager.");
            while (Instance._clients.Count > 0)
            {
                var cl = Instance._clients[0];
                cl.Disconnect();
                Instance._clients.Remove(cl);
            }

            Instance = null;
        }
    }
}