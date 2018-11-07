using System.Net.Sockets;
using Estrella.InterLib.NetworkObjects;
using Estrella.Util;

namespace Estrella.World.InterServer
{
    [ServerModule(InitializationStage.Services)]
    public sealed class ZoneAcceptor : AbstractAcceptor
    {
        public ZoneAcceptor(int port) : base(port)
        {
            OnIncommingConnection += WorldAcceptor_OnIncommingConnection;
            Log.WriteLine(LogLevel.Info, "Listening on port {0} for zones.", port);
        }

        public static ZoneAcceptor Instance { get; private set; }

        private void WorldAcceptor_OnIncommingConnection(Socket session)
        {
            // So something with it X:
            Log.WriteLine(LogLevel.Info, "Incoming connection from {0}", session.RemoteEndPoint);
            var wc = new ZoneConnection(session);
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
                Instance = new ZoneAcceptor(port);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}