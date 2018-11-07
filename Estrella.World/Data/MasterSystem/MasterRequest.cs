using System;
using Estrella.World.Managers;
using Estrella.World.Networking;

namespace Estrella.World.Data.MasterSystem
{
    public class MasterRequest
    {
        #region .ctor

        public MasterRequest(string target, WorldClient pClient)
        {
            InvitedClient = ClientManager.Instance.GetClientByCharname(target);
            if (InvitedClient == null)
                return;

            InviterClient = pClient;
            CrationTimeStamp = DateTime.Now;
        }

        #endregion

        #region Properties

        public DateTime CrationTimeStamp { get; private set; }
        public WorldClient InvitedClient { get; private set; }
        public WorldClient InviterClient { get; private set; }

        #endregion
    }
}