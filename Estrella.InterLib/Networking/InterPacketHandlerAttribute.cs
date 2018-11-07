using System;

namespace Estrella.InterLib.Networking
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InterPacketHandlerAttribute : Attribute
    {
        public InterPacketHandlerAttribute(InterHeader ih)
        {
            Header = ih;
        }

        public InterHeader Header { get; private set; }
    }
}