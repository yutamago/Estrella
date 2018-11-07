using System.Net.Sockets;
using Estrella.InterLib.Networking;

namespace Estrella.InterLib.NetworkObjects
{
    public class AbstractConnector
    {
        protected InterClient client;
        public string IpAddress { get; private set; }
        public int Port { get; private set; }
        public bool Pong { get; private set; }
        public bool ForcedClose { get; private set; }

        public void Connect(string ip, int port)
        {
            IpAddress = ip;
            Port = port;
            ForcedClose = false;
            Connect();
        }

        public void Connect()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(IpAddress, Port);
            client = new InterClient(tcpClient.Client);
            client.OnPacket += ClientOnPacket;
        }

        void ClientOnPacket(object sender, InterPacketReceivedEventArgs e)
        {
            if (e.Packet.OpCode == InterHeader.Ping)
            {
                SendPong();
            }
            else if (e.Packet.OpCode == InterHeader.Pong)
            {
                Pong = true;
            }
        }

        public void SendPing()
        {
            Pong = false;
            using (var packet = new InterPacket(InterHeader.Ping))
            {
                client.SendPacket(packet);
            }
        }

        public void SendPong()
        {
            using (var packet = new InterPacket(InterHeader.Pong))
            {
                client.SendPacket(packet);
            }
        }
    }
}