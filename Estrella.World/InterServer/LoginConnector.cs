using System;
using Estrella.InterLib.Networking;
using Estrella.InterLib.NetworkObjects;
using Estrella.Util;

namespace Estrella.World.InterServer
{
    [ServerModule(InitializationStage.Services)]
    public sealed class LoginConnector : AbstractConnector
    {
        public LoginConnector(string ip, int port)
        {
            try
            {
                Connect(ip, port);
                Log.WriteLine(LogLevel.Info, "Connected to server @ {0}:{1}", ip, port);
                client.OnPacket += ClientOnPacket;
                client.OnDisconnect += ClientOnDisconnect;
                client.SendInterPass(Settings.Instance.InterPassword);
                InterHandler.TryAssiging(this);
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "Couldn't connect to server @ {0}:{1}", ip, port);
                Console.ReadLine();
                Environment.Exit(7);
            }
        }

        public static LoginConnector Instance { get; private set; }

        void ClientOnDisconnect(object sender, SessionCloseEventArgs e)
        {
            Log.WriteLine(LogLevel.Error, "Disconnected from server.");
            client.OnPacket -= ClientOnPacket;
            client.OnDisconnect -= ClientOnDisconnect;
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
            return Load(Settings.Instance.LoginServerIp, Settings.Instance.LoginServerPort);
        }

        public static bool Load(string ip, int port)
        {
            try
            {
                Instance = new LoginConnector(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SendPacket(InterPacket packet)
        {
            client?.SendPacket(packet);
        }
    }
}