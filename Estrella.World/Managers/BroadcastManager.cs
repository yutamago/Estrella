using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Data;

namespace Estrella.World.Managers
{
    [ServerModule(InitializationStage.Clients)]
    public class BroadcastManager
    {
        public static BroadcastManager Instance { get; set; }

        [InitializerMethod]
        public static bool init()
        {
            Instance = new BroadcastManager();
            return true;
        }

        public void BroadcastInRange(WorldCharacter pChar, Packet pPacket, bool ToAll)
        {
            pChar.BroucastPacket(pPacket);
        }
    }
}