using System;
using Estrella.World.Managers;
using Estrella.World.Networking;

namespace Estrella.World.Data.Group
{
    public class GroupRequest
    {
        #region .ctor

        public GroupRequest(WorldClient pFrom, Group pGroup, string pInvited)
        {
            CrationTimeStamp = DateTime.Now;
            InvitedClient = ClientManager.Instance.GetClientByCharname(pInvited);
            InviterClient = pFrom;
            Group = pGroup;
        }

        #endregion

        #region Properties

        public DateTime CrationTimeStamp { get; private set; }
        public Group Group { get; internal set; }
        public WorldClient InvitedClient { get; private set; }
        public WorldClient InviterClient { get; private set; }

        #endregion

        #region Methods

        #endregion
    }
}