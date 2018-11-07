using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.InterLib.Networking;
using Estrella.Zone.InterServer;
using Estrella.Zone.Networking;

namespace Estrella.Zone.Handlers
{
    public sealed class Handler3
    {
        [PacketHandler(CH3Type.BackToCharSelect)]
        public static void BackTo(ZoneClient client, Packet packet)
        {
            using (var iacket = new InterPacket(InterHeader.ClientDisconect))
            {
                iacket.WriteString(client.Character.Character.Name, 16);
                WorldConnector.Instance.SendPacket(iacket);
            }
        }

        public static void SendError(ZoneClient client, ServerError error)
        {
            using (var pack = new Packet(SH3Type.Error))
            {
                pack.WriteShort((byte) error);
                client.SendPacket(pack);
            }
        }
    }
}