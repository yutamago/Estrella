using Estrella.InterLib.Networking;
using Estrella.Util;

namespace Estrella.World.Managers
{
    [ServerModule(InitializationStage.Clients)]
    public class ZoneManager
    {
        public static ZoneManager Instance { get; set; }

        [InitializerMethod]
        public static bool init()
        {
            Instance = new ZoneManager();
            return true;
        }

        public static void Broadcast(InterPacket pPacket)
        {
            foreach (var zone in Program.Zones.Values)
            {
                zone.SendPacket(pPacket);
            }
        }
    }
}