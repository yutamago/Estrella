using System;
using System.Net;
using System.Net.Sockets;

namespace Estrella.InterLib.NetworkObjects
{
    public class AbstractAcceptor
    {
        private readonly TcpListener listener;

        public AbstractAcceptor(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start(5);
            StartReceive();
        }

        public ulong AcceptedClients { get; private set; }
        public event OnIncomingConnectionDelegate OnIncommingConnection;

        private void StartReceive()
        {
            listener.BeginAcceptSocket(EndReceive, null);
        }

        private void EndReceive(IAsyncResult iar)
        {
            var socket = listener.EndAcceptSocket(iar);
            if (socket != null && OnIncommingConnection != null)
            {
                OnIncommingConnection(socket);
            }

            AcceptedClients++;
            StartReceive();
        }
    }
}