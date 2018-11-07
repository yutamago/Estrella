using System;
using System.Net.Sockets;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Managers;

namespace Estrella.World.Networking
{
    [ServerModule(InitializationStage.Networking)]
    public sealed class WorldAcceptor : Listener
    {
        public WorldAcceptor(int port)
            : base(port)
        {
            Start();
            Log.WriteLine(LogLevel.Info, "WorldAcceptor ready at {0}", port);
        }

        private static WorldAcceptor Instance { get; set; }

        public override void OnClientConnect(Socket socket)
        {
            var client = new WorldClient(socket);
            ClientManager.Instance.AddClient(client);
            Log.WriteLine(LogLevel.Debug, "Client connected from {0}", client.Host);
        }

        [InitializerMethod]
        public static bool Load()
        {
            try
            {
                Instance = new WorldAcceptor(Settings.Instance.Port);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "WorldAcceptor exception: {0}", ex.ToString());
                return false;
            }
        }
    }
}