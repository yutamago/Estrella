using System.Net.Sockets;
using Estrella.InterLib.NetworkObjects;
using Estrella.Util;

namespace Estrella.Login.InterServer
{
    [ServerModule(InitializationStage.Services)]
    public sealed class WorldAcceptor : AbstractAcceptor
    {
        public WorldAcceptor(int port) : base(port)
        {
            OnIncommingConnection += WorldAcceptor_OnIncommingConnection;
            Log.WriteLine(LogLevel.Info, "Listening on port {0}", port);
        }

        public static WorldAcceptor Instance { get; private set; }

        private void WorldAcceptor_OnIncommingConnection(Socket session)
        {
            // So something with it X:
            Log.WriteLine(LogLevel.Info, "Incomming connection from {0}", session.RemoteEndPoint);
            var wc = new WorldConnection(session);
        }

        [InitializerMethod]
        public static bool Load()
        {
            return Load(Settings.Instance.InterServerPort);
        }

        public static bool Load(int port)
        {
            try
            {
                Instance = new WorldAcceptor(port);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}