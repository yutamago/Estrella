﻿using System;
using System.Net.Sockets;
using Estrella.FiestaLib.Networking;
using Estrella.Util;

namespace Estrella.Zone.Networking
{
    public sealed class ZoneAcceptor : Listener
    {
        public ZoneAcceptor(int port)
            : base(port)
        {
            Start();
            Log.WriteLine(LogLevel.Info, "Listening at port {0} for incoming clients.", port);
        }

        public static ZoneAcceptor Instance { get; private set; }

        public override void OnClientConnect(Socket socket)
        {
            var client = new ZoneClient(socket);

            //  ClientManager.Instance.AddClient(client); //They register once authenticated now
            Log.WriteLine(LogLevel.Debug, "Client connected from {0}", client.Host);
            // ClientManager.Instance.AddClient(client); //They register once authenticated now
        }

        public static bool Load()
        {
            try
            {
                Instance = new ZoneAcceptor(Program.ServiceInfo.Port);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "ZoneAcceptor exception: {0}", ex.ToString());
                return false;
            }
        }
    }
}