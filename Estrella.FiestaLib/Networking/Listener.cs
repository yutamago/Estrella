using System;
using System.Net;
using System.Net.Sockets;

namespace Estrella.FiestaLib.Networking
{
    public abstract class Listener
    {
        public Listener(int port)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public bool IsRunning { get; private set; }
        public Socket Socket { get; private set; }

        private void EndAccept(IAsyncResult ar)
        {
            if (!IsRunning) return;
            try
            {
                var newclient = Socket.EndAccept(ar);
                OnClientConnect(newclient);
            }
            finally
            {
                Socket.BeginAccept(EndAccept, null);
            }
        }

        public void Stop()
        {
            Socket.Close();
            IsRunning = false;
        }

        public void Start()
        {
            Socket.Listen(10);
            IsRunning = true;
            Socket.BeginAccept(EndAccept, null);
        }

        public abstract void OnClientConnect(Socket socket);
    }
}