using System;

namespace Estrella.FiestaLib.Networking
{
    public sealed class SessionCloseEventArgs : EventArgs
    {
        public static readonly SessionCloseEventArgs RemoteHostDisconnected =
            new SessionCloseEventArgs("The remote host disconnected.");

        public static readonly SessionCloseEventArgs ConnectionTerminated =
            new SessionCloseEventArgs("The connection was forcibly terminated.");

        public SessionCloseEventArgs(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }
}