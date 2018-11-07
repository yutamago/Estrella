using System;
using Estrella.World.Managers;
using Estrella.World.Networking;

namespace Estrella.World.Data.MasterSystem
{
    public class MasterRequest
    {
        public MasterRequest(string target, WorldClient pClient)
        {
            InvitedClient = ClientManager.Instance.GetClientByCharname(target);
            if (InvitedClient == null)
                return;

            InviterClient = pClient;
            CrationTimeStamp = DateTime.Now;
        }

        public DateTime CrationTimeStamp { get; private set; }
        public WorldClient InvitedClient { get; private set; }
        public WorldClient InviterClient { get; private set; }
    }
}