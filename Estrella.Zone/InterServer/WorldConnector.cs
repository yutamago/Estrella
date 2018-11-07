﻿using System;
using System.Threading;
using Estrella.InterLib.Networking;
using Estrella.InterLib.NetworkObjects;
using Estrella.Util;

namespace Estrella.Zone.InterServer
{
    [ServerModule(InitializationStage.Services)]
    public sealed class WorldConnector : AbstractConnector
    {
        public WorldConnector(string ip, int port)
        {
            try
            {
                ConnectAndAssign(ip, port);
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "Couldn't connect to server @ {0}:{1}", ip, port);
                Console.ReadLine();
                Environment.Exit(7);
            }
        }

        public static WorldConnector Instance { get; private set; }

        private void ConnectAndAssign(string ip, int port)
        {
            Connect(ip, port);
            Log.WriteLine(LogLevel.Info, "Connected to server @ {0}:{1}", ip, port);
            client.OnPacket += ClientOnPacket;
            client.OnDisconnect += ClientOnDisconnect;
            client.SendInterPass(Settings.Instance.InterPassword);
            InterHandler.TryAssiging(this);
        }

        void ClientOnDisconnect(object sender, SessionCloseEventArgs e)
        {
            Log.WriteLine(LogLevel.Error, "Disconnected from server.");
            client.OnPacket -= ClientOnPacket;
            client.OnDisconnect -= ClientOnDisconnect;
            if (!Program.Shutdown)
            {
                // Try reconnect
                while (true)
                {
                    try
                    {
                        ConnectAndAssign(Settings.Instance.WorldServerIP, Settings.Instance.WorldServerPort);
                        break;
                    }
                    catch
                    {
                        Log.WriteLine(LogLevel.Warn, "Trying to reconnect in 5 seconds.");
                        Thread.Sleep(5000);
                    }
                }

                Log.WriteLine(LogLevel.Warn, "We should be up again :)");
            }
        }

        void ClientOnPacket(object sender, InterPacketReceivedEventArgs e)
        {
            try
            {
                var method = InterHandlerStore.GetHandler(e.Packet.OpCode);
                if (method != null)
                {
                    var action = InterHandlerStore.GetCallback(method, this, e.Packet);
                    if (Worker.Instance == null)
                    {
                        action();
                    }
                    else
                    {
                        Worker.Instance.AddCallback(action);
                    }
                }
                else
                {
                    Log.WriteLine(LogLevel.Debug, "Unhandled packet: {0}", e.Packet);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, ex.ToString());
            }
        }

        [InitializerMethod]
        public static bool Load()
        {
            return Load(Settings.Instance.WorldServerIP, Settings.Instance.WorldServerPort);
        }

        public static bool Load(string ip, int port)
        {
            try
            {
                Instance = new WorldConnector(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SendPacket(InterPacket packet)
        {
            if (client == null) return;
            client.SendPacket(packet);
        }

        public void Disconnect()
        {
            if (client == null) return;
            client.Disconnect();
        }
    }
}