using System;
using System.Net.Sockets;
using Estrella.FiestaLib.Networking;
using Estrella.Login.Handlers;
using Estrella.Util;

namespace Estrella.Login.Networking
{
    public sealed class LoginClient : Client
    {
        public LoginClient(Socket sock)
            : base(sock)
        {
            OnPacket += LoginClient_OnPacket;
            OnDisconnect += LoginClient_OnDisconnect;
        }

        public bool IsAuthenticated { get; set; }
        public int AccountID { get; set; }
        public string Username { get; set; }
        public byte Admin { get; set; }
        public bool IsTransferring { get; set; }

        public ClientTransfer GenerateTransfer()
        {
            if (!IsAuthenticated) return null;
            var hash = Guid.NewGuid().ToString().Replace("-", "");
            return new ClientTransfer(AccountID, Username, 0, Admin, Host, hash);
        }

        void LoginClient_OnDisconnect(object sender, SessionCloseEventArgs e)
        {
            Log.WriteLine(LogLevel.Debug, "{0} Disconnected.", Host);
            ClientManager.Instance.RemoveClient(this);
        }

        void LoginClient_OnPacket(object sender, PacketReceivedEventArgs e)
        {
            var method = HandlerStore.GetHandler(e.Packet.Header, e.Packet.Type);
            if (method != null)
            {
                var action = HandlerStore.GetCallback(method, this, e.Packet);
                Worker.Instance.AddCallback(action);
            }
            else
            {
                Log.WriteLine(LogLevel.Debug, "Header:{0} -> Type:{1}", e.Packet.Header, e.Packet.Type);
                Log.WriteLine(LogLevel.Debug, "Unhandled packet: {0}", e.Packet);
            }
        }
    }
}