namespace Estrella.Zone.Game.Group
{
    public class GroupMember
    {
        public GroupMember()
        {
            IsMaster = false;
            IsOnline = false;
            Group = null;
            Name = "";
            Character = null;
            IsReadyForUpdates = false;
        }

        public GroupMember(string pName, bool pIsMaster, bool pIsOnline)
        {
            Name = pName;
            IsMaster = pIsMaster;
            IsOnline = pIsOnline;
        }

        public bool IsMaster { get; internal set; }
        public bool IsOnline { get; set; }
        public Group Group { get; internal set; }
        public string Name { get; internal set; }
        public ZoneCharacter Character { get; internal set; }

        public bool IsReadyForUpdates { get; set; }
    }
}