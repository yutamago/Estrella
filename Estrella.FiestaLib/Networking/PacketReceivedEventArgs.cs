using System;

namespace Estrella.FiestaLib.Networking
{
    public sealed class PacketReceivedEventArgs : EventArgs
    {
        public PacketReceivedEventArgs(Packet packet)
        {
            Packet = packet;
        }

        public Packet Packet { get; private set; }
    }
}