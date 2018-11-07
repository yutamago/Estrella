using System;

namespace Estrella.InterLib.Networking
{
    public sealed class InterPacketReceivedEventArgs : EventArgs
    {
        public InterPacketReceivedEventArgs(InterPacket packet, InterClient client)
        {
            Packet = packet;
            Client = client;
        }

        public InterPacket Packet { get; private set; }
        public InterClient Client { get; private set; }
    }
}