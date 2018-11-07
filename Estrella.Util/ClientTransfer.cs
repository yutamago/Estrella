using System;

namespace Estrella.Util
{
    public sealed class ClientTransfer
    {
        public ClientTransfer(int accountID, string userName, int CharID, byte admin, string hostIP, string hash)
        {
            Type = TransferType.World;
            AccountID = accountID;
            Username = userName;
            this.CharID = CharID;
            Admin = admin;
            HostIP = hostIP;
            Hash = hash;
            Time = DateTime.Now;
        }

        public ClientTransfer(int accountID, string userName, string charName, int CharID, ushort randid, byte admin,
            string hostIP)
        {
            Type = TransferType.Game;
            AccountID = accountID;
            Username = userName;
            Admin = admin;
            HostIP = hostIP;
            CharacterName = charName;
            this.CharID = CharID;
            RandID = randid;
            Time = DateTime.Now;
        }

        public string Hash { get; private set; }
        public ushort RandID { get; private set; }
        public string CharacterName { get; private set; }
        public int CharID { get; private set; }
        public int AccountID { get; private set; }
        public byte Admin { get; private set; }
        public string Username { get; private set; }
        public string HostIP { get; private set; }
        public DateTime Time { get; private set; }
        public TransferType Type { get; private set; }
    }
}